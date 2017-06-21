using BeepBoopBot.Services;
using Discord.Commands;
using System.Runtime.CompilerServices;

namespace BeepBoopBot.Attributes
{
    public class BotModule : NameAttribute
    {
        public BotModule(string Name) : base(Name)
        {
        }
    }
}
