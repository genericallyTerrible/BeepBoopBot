using Discord;
using Discord.Commands;
using BeepBoopBot.Preconditions;
using BeepBoopBot.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeepBoopBot.Extensions;

namespace BeepBoopBot.Modules.Help
{
    [BotModule("Help")]
    [MinPermissions(BotAccessLevel.User)]
    public class Help : ModuleBase<ShardedCommandContext>
    {
        private CommandService Service;

        public Help(CommandService service)           // Create a constructor for the commandservice dependency
        {
            Service = service;
        }

        [BotCommand, Usage, Description, Aliases]
        public async Task Help0()
        {
            Configuration config = Configuration.Load();

            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Color = config.EmbedColor,
                Description = "Here's a list of my commands!"
            };

            IEnumerable<ModuleInfo> modules = Service.Modules.Where(module => !module.IsSubmodule); // List of project modules.

            foreach (ModuleInfo module in modules) // Iterate over modules
            {
                // Recursively generates module fields for top level modules
                List<EmbedFieldBuilder> moduleFields = await module.BuildEmbedField(Context, config.Prefix, SubmoduleInclusion.Distinct);
                foreach (EmbedFieldBuilder moduleField in moduleFields) // If there was a module field returned, add it to the builder.
                    embedBuilder.AddField(moduleField);
            }
            await ReplyAsync("", embed:embedBuilder.Build());
        }

        [BotCommand, Usage, Description, Aliases]
        public async Task Help1
        (
            [Parameter(0), Remainder]
            string command
        )
        {
            Configuration config = Configuration.Load();
            // Trim the prefix if present
            if (command.Substring(0, config.Prefix.Length).Equals(config.Prefix))
            {
                command = command.Substring(config.Prefix.Length);
            }

            SearchResult result = Service.Search(Context, command);
            if (result.IsSuccess)
            {
                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Color = config.EmbedColor
                    //Description = $"Here are some commands like `{command}`"
                };

                foreach (CommandMatch match in result.Commands)
                {
                    CommandInfo cmdMatch = match.Command;
                    if (await cmdMatch.IsAccessible(Context))
                    {
                        string fieldName = cmdMatch.Description(config.Prefix);
                        string parameters = cmdMatch.ParameterDescription();
                        embedBuilder.AddField(field =>
                        {
                            field.Name = fieldName;

                            field.Value = (
                            (cmdMatch.Summary?   .Count() > 0 ? ($"**Summary**\n\t{   string.Join(", ", cmdMatch.Summary)}\n") : ("")) +
                            (cmdMatch.Aliases?   .Count() > 0 ? ($"**Aliases**\n\t{   string.Join(", ", cmdMatch.Aliases)}\n") : ("")) +
                            (cmdMatch.Parameters?.Count() > 0 ? ($"**Parameters**\n\t{                  parameters       }\n") : ("")) +
                            (cmdMatch.Remarks?   .Count() > 0 ? ($"**Usage**\n\t{string.Format(cmdMatch.Remarks, config.Prefix, cmdMatch.Aliases.First())}\n") : (""))  
                            .Trim());

                            field.IsInline = true;
                        });
                    }
                }

                if (embedBuilder.Fields.Count > 0)
                {
                    await ReplyAsync($"Here are some commands like `{command}`", embed: embedBuilder.Build());
                    return;
                }
            }

            await ReplyAsync($"Sorry, I couldn't find a command like `{command}`.");
        }

        [Command("modules")]
        public async Task Modules()
        {
            Configuration config = Configuration.Load();

            IEnumerable<string> moduleNames = Service.Modules.Where(m => m.IsAccessible(Context).Result).Select(n => n.Name).Distinct();

            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Color = config.EmbedColor,
                Title = "Here's a list of my modules!",
                Description = "**" + string.Join("**\n**", moduleNames) + 
                    "**\n\nUse `cmds moduleName` for a list of commands in a given module"
            };

            await ReplyAsync("", embed: embedBuilder.Build());
        }

        [Command("cmds")]
        public async Task ModuleCommands
        (
            [Remainder]
            string moduleName
        )
        {
            ModuleInfo module = Service.Modules.FirstOrDefault(m => m.Name.ToLowerInvariant().Equals(moduleName.ToLowerInvariant()));

            if (module != null)
            {
                Configuration config = Configuration.Load();

                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Color = config.EmbedColor,
                    Description = $"Here's a list of commands in {module.Name}!"
                };

                List<EmbedFieldBuilder> moduleFields = await module.BuildEmbedField(Context, config.Prefix, SubmoduleInclusion.Distinct);
                foreach (EmbedFieldBuilder moduleField in moduleFields) // If there was a module field returned, add it to the builder.
                    embedBuilder.AddField(moduleField);

                if (embedBuilder.Fields.Count > 0)
                {
                    await ReplyAsync("", embed: embedBuilder.Build());
                    return;
                }
            }

            await ReplyAsync($"I couldn't find any modules like {moduleName}");
        }
    }
}
