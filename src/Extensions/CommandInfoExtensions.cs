using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeepBoopBot.Extensions
{
    public static class CommandInfoExtensions
    {
        /// <summary>
        /// Checks to see if the command is accessable in the current context.
        /// </summary>
        /// <param name="command"> The command to be checked. </param>
        /// <param name="context"> The context to check the command in. </param>
        /// <returns> Returns true if the command is accessable, false otherwise. </returns>
        public static async Task<bool> IsAccessible(this CommandInfo command, ICommandContext context)
        {
            PreconditionResult result = await command.CheckPreconditionsAsync(context);
            return result.IsSuccess;
        }

        /// <summary>
        /// Builds a description of the command and prepends a prefix if specified.
        /// </summary>
        /// <param name="command"> The command to be described. </param>
        /// <param name="prefix"> The specified prefix for this command. </param>
        /// <returns> Retruns a one line code block containting a string representation of the command and its parameters. </returns>
        public static string Description(this CommandInfo command, string prefix = "")
        {
            StringBuilder descriptionBuilder = new StringBuilder();

            descriptionBuilder.Append($"`{prefix}{command.Aliases[0]}");
            IReadOnlyList<ParameterInfo> parameters = command.Parameters;
            if (parameters.Count > 0)
            {
                descriptionBuilder.Append(
                    " <" +                      // Lists all parameters with comma seperation
                    string.Join(", ", command.Parameters.Select(p => p.Name)) +
                    ">");
            }
            descriptionBuilder.Append("`");

            return descriptionBuilder.ToString();
        }

        /// <summary>
        /// Builds a description of the command's parameters
        /// </summary>
        /// <param name="command"> The command whose parameters are to be described. </param>
        /// <returns> Returns a string containing a multiline list of the parameters and their descriptions. </returns>
        public static string ParameterDescription(this CommandInfo command)
        {
            StringBuilder paramBuilder = new StringBuilder();
            foreach (ParameterInfo parameter in command.Parameters)
            {
                paramBuilder.Append($"{parameter.Name}: {parameter.Summary}\n\t");
            }
            return paramBuilder.ToString().Trim();
        }
    }
}
