using BeepBoopBot.Attributes;
using BeepBoopBot.Preconditions;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [Name("Math")]
    [MinPermissions(BotAccessLevel.User)]
    public class Math : ModuleBase<ShardedCommandContext>
    {
        [BotCommand, Usage, Description, Aliases]
        public async Task IsInteger
        (
            [Parameter(0)]
            int number
        )
        {
            await ReplyAsync($"The text {number} is a number!");
        }

        [BotCommand, Usage, Description, Aliases]
        public async Task Multiply
        (
            [Parameter(0)]
            int a,
            [Parameter(1)]
            int b
        )
        {
            int product = a * b;
            await ReplyAsync($"The product of `{a} * {b}` is `{product}`.");
        }

        [BotCommand, Usage, Description, Alias]
        public async Task Addmany
        (
            [Parameter(0)]
            params int[] numbers
        )
        {
            int sum = numbers.Sum();
            await ReplyAsync($"The sum of `{string.Join(", ", numbers)}` is `{sum}`.");
        }
    }
}
