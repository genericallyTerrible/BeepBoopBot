using Discord.Commands;
using BeepBoopBot.Preconditions;
using System.Linq;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [Name("Math")]
    [MinPermissions(BotAccessLevel.User)]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        [Command("isinteger")]
        [Remarks("Check if the input text is a whole number.")]
        public async Task IsInteger(int number)
        {
            await ReplyAsync($"The text {number} is a number!");
        }

        [Command("multiply")]
        [Remarks("Get the product of two numbers.")]
        public async Task Multiply(int a, int b)
        {
            int product = a * b;
            await ReplyAsync($"The product of `{a} * {b}` is `{product}`.");
        }

        [Command("addmany")]
        [Remarks("Get the sum of many numbers")]
        public async Task Addmany(params int[] numbers)
        {
            int sum = numbers.Sum();
            await ReplyAsync($"The sum of `{string.Join(", ", numbers)}` is `{sum}`.");
        }
    }
}
