using Discord;
using Discord.Commands;
using BeepBoopBot.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [Name("Help Commands")]
    [MinPermissions(BotAccessLevel.User)]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;

        public HelpModule(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        [Command("help")]
        [Alias("halp", "helpme")]
        public async Task HelpAsync()
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

        [Command("help")]
        [Alias("halp", "helpme")]
        public async Task HelpAsync(
            [Summary("The command you want help on"),Remainder]
            string command
            )
        {
            SearchResult result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like `{command}`.");
                return;
            }

            string prefix = Configuration.Load().Prefix;
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Color = Configuration.Load().EmbedColor,
                Description = $"Here are some commands like `{command}`"
            };

            //List<CommandMatch> res = result.Commands.ToList();

            foreach (CommandMatch match in result.Commands)
            {
                CommandInfo cmdMatch = match.Command;

                string name = await CommandDescriptionBuilder(cmdMatch, prefix);
                if (name?.Count() > 0)
                {

                    embedBuilder.AddField(field =>
                    {
                        field.Name = name;

                        field.Value = (
                        (cmdMatch.Aliases?   .Count() > 0 ? ($"Aliases: {   string.Join(", ", cmdMatch.Aliases)   }\n") : ("")) +
                        (cmdMatch.Parameters?.Count() > 0 ? ($"Parameters: {string.Join(", ", cmdMatch.Parameters)}\n") : ("")) +
                        (cmdMatch.Summary?   .Count() > 0 ? ($"Summary: {   string.Join(", ", cmdMatch.Summary)   }\n") : ("")) +
                        (cmdMatch.Remarks?   .Count() > 0 ? ($"Remarks: {                     cmdMatch.Remarks    }\n") : (""))
                        .Trim());

                        field.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, embedBuilder.Build());
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
                descriptionBuilder.Append(await CommandDescriptionBuilder(command, prefix));
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

                //descriptionBuilder.Append("Preconditions: ");
                //if (command.Preconditions.Count > 0)
                //{
                //    descriptionBuilder.Append(string.Join(", ", command.Preconditions.Select(p => p.ToString())));
                //}
                //else
                //{
                //    descriptionBuilder.Append("None");
                //}
                //descriptionBuilder.AppendLine();
            }

            return descriptionBuilder.ToString();
        }
    }
}
