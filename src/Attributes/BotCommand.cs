using BeepBoopBot.Services;
using Discord.Commands;
using System.Runtime.CompilerServices;

namespace BeepBoopBot.Attributes
{
    public class BotCommand : CommandAttribute
    {
        public BotCommand([CallerMemberName] string memberName = "") : base(Localization.LoadCommandString(memberName.ToLowerInvariant() + "_cmd").Split(' ')[0])
        {
        }
    }
}
