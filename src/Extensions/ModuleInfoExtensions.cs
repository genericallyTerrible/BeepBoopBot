using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeepBoopBot.Extensions
{
    public static class ModuleInfoExtensions
    {
        public static async Task<bool> IsAccessible(this ModuleInfo module, ICommandContext context)
        {
            return (await module.NumAccessibleCommands(context) > 0);
        }

        public static async Task<int> NumAccessibleCommands(this ModuleInfo module, ICommandContext context)
        {
            int i = 0;
            foreach(CommandInfo cmd in module.Commands)
            {
                if (await cmd.IsAccessible(context))
                    i++;
            }
            return i;
        }

        public static async Task<List<string>> GetCommands
        (
            this ModuleInfo module, ICommandContext context, string prefix = "",
            SubmoduleInclusion inclusion = SubmoduleInclusion.None
        )
        {
            List<string> commands = new List<string>();
            // Iterate over commands directly contained by this module.
            foreach (CommandInfo command in module.Commands)
            {
                if (await command.IsAccessible(context))
                    commands.Add(command.Description(prefix));
            }

            if (inclusion != SubmoduleInclusion.None)
            {
                IEnumerable<ModuleInfo> submodules;
                if (inclusion == SubmoduleInclusion.Override)
                {
                    // Include all commands from all submodules.
                    submodules = module.Submodules;
                }
                else
                {
                    // Only include commands from modules that have the same name.
                    submodules = module.Submodules.Where(m => m.Name.Equals(module.Name));
                }
                // Add the commands from all the included submodules following the same rules.
                foreach (ModuleInfo submodule in submodules)
                {
                    commands.AddRange(await submodule.GetCommands(context, prefix, inclusion));
                }
            }

            return commands;
        }

        public static async Task<List<EmbedFieldBuilder>> BuildEmbedField
        (
            this ModuleInfo module, ICommandContext context, string prefix = "",
            SubmoduleInclusion inclusion = SubmoduleInclusion.None, bool isInline = false
        )
        {
            List<EmbedFieldBuilder> moduleFields = new List<EmbedFieldBuilder>();

            // Generates a list of commands following the prescribed rules.
            List<string> commands = await module.GetCommands(context, prefix, inclusion);
            if (commands.Count > 0)
            {
                moduleFields.Add(new EmbedFieldBuilder()
                {
                    Name = module.Name,
                    Value = string.Join("\n", commands),
                    IsInline = isInline
                });
            }

            // If submodules are not  distinct, their commands are alread in the commands list
            if (inclusion == SubmoduleInclusion.Distinct)
            {
                foreach (ModuleInfo submodule in module.GetDistinctSubmodules(inclusion))
                {
                    moduleFields.AddRange(await submodule.BuildEmbedField(context, prefix, inclusion));
                }
            }

            return moduleFields;
        }

        /// <summary>
        /// Returns a list of submodules whose names are different from their parent's name.
        /// It should be noted that modules whose names are the same as their grandparents's are considered distinct.
        /// </summary>
        public static List<ModuleInfo> GetDistinctSubmodules(this ModuleInfo module, SubmoduleInclusion inclusion = SubmoduleInclusion.None)
        {
            List<ModuleInfo> distinctSubmodules = new List<ModuleInfo>();

            distinctSubmodules.AddRange(module.Submodules.Where(m => !m.Name.Equals(module.Name)).ToList());

            if (inclusion != SubmoduleInclusion.None)
            {
                foreach (ModuleInfo submodule in module.Submodules.Where(m => m.Name.Equals(module.Name)))
                {
                    distinctSubmodules.AddRange(submodule.GetDistinctSubmodules(inclusion));
                }
            }

            return distinctSubmodules;
        }
    }
}
