using BeepBoopBot.Attributes;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules.Moderation
{
    [BotModule("Moderation")]
    [RequireContext(ContextType.Guild)]
    public class Moderation : ModuleBase<ShardedCommandContext>
    {
        [BotCommand, Usage, Description, Aliases]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        [RequireBotPermission(Discord.GuildPermission.KickMembers)]
        public async Task Kick
        (
            [Parameter(0), Remainder]
            SocketGuildUser user
        )
        {
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if (moderator.Hierarchy > user.Hierarchy)
            {

                try
                {
                    await user.KickAsync();                         // Attempt to kick before sending message
                    await ReplyAsync($"cya {user.Mention} :wave:"); // If an error occurs in kicking, no awkward waves
                }
                catch
                {
                    await ReplyAsync($"Whoops, I couldn't kick {user.Mention}");
                }
            }
            else
            {
                await ReplyAsync($"Sorry, you can't kick {user.Mention}");
            }
        }


    }
}
