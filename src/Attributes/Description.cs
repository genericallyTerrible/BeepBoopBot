using BeepBoopBot.Services;
using Discord.Commands;
using System.Runtime.CompilerServices;

namespace BeepBoopBot.Attributes
{
    public class Description : SummaryAttribute
    {
        public Description([CallerMemberName] string memberName = "") : base(Localization.LoadCommandString(memberName.ToLowerInvariant() + "_desc"))
        {
        }
    }
}
