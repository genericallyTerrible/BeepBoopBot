using BeepBoopBot.Services;
using Discord.Commands;
using System.Runtime.CompilerServices;

namespace BeepBoopBot.Attributes
{
    public class Usage : RemarksAttribute
    {
        public Usage([CallerMemberName] string memberName = "") : base(Localization.LoadCommandString(memberName.ToLowerInvariant() + "_usage"))
        {
        }
    }
}
