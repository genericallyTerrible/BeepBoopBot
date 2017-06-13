using BeepBoopBot.Services;
using Discord.Commands;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BeepBoopBot.Attributes
{
    public class Aliases : AliasAttribute
    {
        public Aliases([CallerMemberName] string memberName = "") : base(Localization.LoadCommandString(memberName.ToLowerInvariant() + "_cmd").Split(' ').Skip(1).ToArray())
        {
        }
    }
}
