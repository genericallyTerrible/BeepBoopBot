using BeepBoopBot.Services;
using Discord.Commands;
using System.Runtime.CompilerServices;

namespace BeepBoopBot.Attributes
{
    class Parameter : SummaryAttribute
    {
        public Parameter(int paramNum, [CallerMemberName] string memberName = "") : base(Localization.LoadCommandString(memberName.ToLowerInvariant() + "_param" + paramNum))
        {
        }
    }
}
