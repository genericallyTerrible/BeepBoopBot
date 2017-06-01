using Discord.Commands;
using Discord.WebSocket;
using BeepBoopBot.Preconditions;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [Name("Moderator")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.ServerMod)]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [Remarks("Kick the specified user.")]
        public async Task Kick([Remainder]SocketGuildUser user)
        {
            await user.KickAsync();                         // Attempt to kick before sending message
            await ReplyAsync($"cya {user.Mention} :wave:"); // If an error occurs in kicking, no awkward waves
        }
    }
}
