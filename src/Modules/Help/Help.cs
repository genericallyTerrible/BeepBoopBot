using Discord;
using Discord.Commands;
using BeepBoopBot.Preconditions;
using BeepBoopBot.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules.Help
{
    [Name("Help Commands")]
    [MinPermissions(BotAccessLevel.User)]
    public class HelpModule : ModuleBase<ShardedCommandContext>
    {
        private CommandService _service;

        public HelpModule(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        [BotCommand, Usage, Description, Aliases]
        public async Task Help()
        {
            string prefix = Configuration.Load().Prefix;
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Color = Configuration.Load().EmbedColor,
                Description = "Here's a list of my commands!"
            };

            IEnumerable<ModuleInfo> modules = _service.Modules.Where(module => !module.IsSubmodule); // List of project modules.

            foreach (ModuleInfo module in modules) // Iterate over modules
            {
                // Recursively generates module fields for top level modules
                List<EmbedFieldBuilder> moduleFields = await ModuleDescriptionBuilder(module, prefix);
                foreach (EmbedFieldBuilder moduleField in moduleFields) // If there was a module field returned, add it to the builder.
                    embedBuilder.AddField(moduleField);
            }

            await ReplyAsync("", false, embedBuilder.Build());
        }

        [BotCommand, Usage, Description, Aliases]
        public async Task HelpWith
        (
        [Summary("The command you want help on."),Remainder]
        string command
        )
        {
            string prefix = Configuration.Load().Prefix;
            if (command.Substring(0, prefix.Length).Equals(prefix))
            {
                //Trim the prefix if present
                command = command.Substring(prefix.Length);
            }
            SearchResult result = _service.Search(Context, command);

            if (result.IsSuccess)
            {
                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Color = Configuration.Load().EmbedColor,
                    Description = $"Here are some commands like `{command}`"
                };

                foreach (CommandMatch match in result.Commands)
                {
                    CommandInfo cmdMatch = match.Command;

                    string fieldName = await CommandDescriptionBuilder(cmdMatch, prefix);
                    if (fieldName?.Count() > 0)
                    {
                        string parameters = await CommandParameterBuilder(cmdMatch);
                        embedBuilder.AddField(field =>
                        {
                            field.Name = fieldName;

                            field.Value = (
                            (cmdMatch.Aliases?   .Count() > 0 ? ($"**Aliases**\n\t{   string.Join(", ", cmdMatch.Aliases)    }\n") : ("")) +
                            (cmdMatch.Parameters?.Count() > 0 ? ($"**Parameters**\n\t{                  parameters           }\n") : ("")) +
                            (cmdMatch.Summary?   .Count() > 0 ? ($"**Summary**\n\t{   string.Join(", ", cmdMatch.Summary)    }\n") : ("")) +
                            (cmdMatch.Remarks?   .Count() > 0 ? ($"**Usage**\n\t{     string.Format(cmdMatch.Remarks, prefix)}\n") : (""))  
                            .Trim());

                            field.IsInline = false;
                        });
                        //embedBuilder.AddField(fb => fb.WithName(GetText("usage")).WithValue(string.Format(com.Remarks, com.Module.Aliases.First())).WithIsInline(false));
                    }
                }
                if (embedBuilder.Fields.Count > 0)
                {
                    await ReplyAsync("", false, embedBuilder.Build());
                    return;
                }
            }
            await ReplyAsync($"Sorry, I couldn't find a command like `{command}`.");
        }

        /// <summary>
        /// Recursively builds a List of EmbedFieldBuilders where the zeroth element in the list is the specified module
        /// and all others are submodules that should be displayed as their own module.
        /// </summary>
        /// <param name="module"> The base module to be used as the "seed". </param>
        /// <param name="prefix"> The server's command prefix </param>
        /// <returns> A List of EmbedFieldBuilder that contains the base module and its submodules where the submodule name differs from the base module. </returns>
        private async Task<List<EmbedFieldBuilder>> ModuleDescriptionBuilder(ModuleInfo module, string prefix)
        {
            List<EmbedFieldBuilder> moduleField = new List<EmbedFieldBuilder>();
            List<EmbedFieldBuilder> submoduleFieldBuilder = new List<EmbedFieldBuilder>();
            StringBuilder descriptionBuilder = new StringBuilder();

            // Iterate over commands directly contained by this module
            foreach (CommandInfo command in module.Commands)
            {
                descriptionBuilder.Append(await CommandDescriptionBuilder(command, prefix));
            }
            // If a submodule has the same name as its parrent, treat its commands as if they were in the parent module
            foreach (ModuleInfo submodule in module.Submodules.Where(m => m.Name.Equals(module.Name)))
            {
                Tuple<string, List<EmbedFieldBuilder>> results = await SubmoduleDescriptionBuilder(submodule, prefix);
                descriptionBuilder.Append(results.Item1);
                // If a submodule's submodule has a different name, treat it like it's own module
                foreach (EmbedFieldBuilder submoduleField in results.Item2)
                {
                    //Store the submodule's fields for later
                    submoduleFieldBuilder.Add(submoduleField);
                }
            }
            // If the caller had access to any commands in this module, append them
            if (descriptionBuilder.Length > 0)
            {
                moduleField.Add(new EmbedFieldBuilder()
                {
                    Name = module.Name,
                    Value = descriptionBuilder.ToString(),
                    IsInline = false
                });
            }
            // Add any submodules from submodules to the main field
            foreach (EmbedFieldBuilder submoduleField in submoduleFieldBuilder)
            {
                moduleField.Add(submoduleField);
            }
            // If a submodule has a different name from its parrent, treat it as its own module
            foreach (ModuleInfo submodule in module.Submodules.Where(m => !m.Name.Equals(module.Name)))
            {
                List<EmbedFieldBuilder> submoduleFields = await ModuleDescriptionBuilder(submodule, prefix);
                foreach (EmbedFieldBuilder field in submoduleFields) // If there was a module field returned, add it to the builder.
                    moduleField.Add(field);
            }
            return moduleField;
        }

        /// <summary>
        /// Recursively builds an EmbedField for the specified module and its submodules.
        /// </summary>
        /// <param name="module"> The base module to be used as the "seed". </param>
        /// <param name="prefix"> The server's command prefix </param>
        /// <returns> A tuple containing a string and a List of EmbedFieldBuilder.
        /// The string contains all submodule commands where the submodule has the same this as the base module.
        /// The List of EmbedFieldBuilder contains all submodules where the submodule name differs from the base module.
        /// </returns>
        private async Task<Tuple<string, List<EmbedFieldBuilder>>> SubmoduleDescriptionBuilder(ModuleInfo module, string prefix)
        {
            StringBuilder descriptionBuilder = new StringBuilder();
            List<EmbedFieldBuilder> submoduleFieldBuilder = new List<EmbedFieldBuilder>();

            // Iterate over commands directly contained by this module
            foreach (CommandInfo command in module.Commands)
            {
                string cmdDesc = await CommandDescriptionBuilder(command, prefix);
                if (!string.IsNullOrWhiteSpace(cmdDesc))
                {
                    descriptionBuilder.Append(cmdDesc);
                }
            }
            // If a submodule has the same name as its parrent, treat its commands as if they were in the parent module
            foreach (ModuleInfo submodule in module.Submodules.Where(sub => sub.Name.Equals(module.Name)))
            {
                Tuple<string, List<EmbedFieldBuilder>> results = await SubmoduleDescriptionBuilder(submodule, prefix);
                descriptionBuilder.Append(results.Item1);
            }
            // If a submodule has a different name from its parrent, treat it as its own module
            foreach (ModuleInfo submodule in module.Submodules.Where(sub => !sub.Name.Equals(module.Name)))
            {
                List<EmbedFieldBuilder> submoduleFields = await ModuleDescriptionBuilder(submodule, prefix);
                foreach (EmbedFieldBuilder field in submoduleFields)
                    submoduleFieldBuilder.Add(field);
            }
            // Return the results as a tuple
            return new Tuple<string, List<EmbedFieldBuilder>>(descriptionBuilder.ToString(), submoduleFieldBuilder);
        }

        /// <summary> If the user has access to the command, build a string representation of it </summary>
        private async Task<string> CommandDescriptionBuilder(CommandInfo command, string prefix)
        {
            StringBuilder descriptionBuilder = new StringBuilder();
            PreconditionResult result = await command.CheckPreconditionsAsync(Context);
            if (result.IsSuccess)
            {
                descriptionBuilder.Append($"`{prefix}{command.Aliases[0]}");
                IReadOnlyList<ParameterInfo> parameters = command.Parameters;
                if (parameters.Count > 0)
                {
                    descriptionBuilder.Append(
                        " <" +                      // Lists all parameters with comma seperation
                        string.Join(", ", command.Parameters.Select(p => p.Name)) +
                        ">");
                }
                descriptionBuilder.AppendLine("`");
            }

            return descriptionBuilder.ToString();
        }

        private async Task<string> CommandParameterBuilder(CommandInfo command)
        {
            StringBuilder paramBuilder = new StringBuilder();
            PreconditionResult result = await command.CheckPreconditionsAsync(Context);
            if (result.IsSuccess)
            {
                foreach (ParameterInfo parameter in command.Parameters)
                {
                    paramBuilder.Append($"{parameter.Name}: {parameter.Summary}\n\t");
                }
            }
            return paramBuilder.ToString().Trim();
        }
    }
}
