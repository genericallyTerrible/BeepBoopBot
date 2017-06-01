using Discord.Commands;
using Discord.WebSocket;
using BeepBoopBot.Preconditions;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [Name("Bot Maintenance")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.BotMaster)]
    public class MaintenanceModule : ModuleBase<SocketCommandContext>
    {
        [Group("add"), Name("Bot Maintenance")]
        public class Add : ModuleBase
        {
            [Command("owner")]
            [Remarks("Adds the specified user as one of my owners.")]
            public async Task AddOwner([Remainder]SocketGuildUser user)
            {
                Configuration config = Configuration.Load();
                if (user.Id == config.BotMaster)
                {
                    await ReplyAsync($"{user.Mention} already loves me plenty. :heart:");
                }
                else if (user.IsBot)
                {
                    await ReplyAsync("Sorry, no :robot: can own me.");
                }
                else
                {
                    if (config.Owners.Contains(user.Id))
                    {
                        await ReplyAsync($"{user.Mention} is already an owner!");
                    }
                    else
                    {
                        config.Owners.Add(user.Id);
                        config.SaveJson();
                        await ReplyAsync($"{user.Mention} is now an owner! :thumbsup:");
                    }
                }
            }
        }

        [Group("remove"), Name("Bot Maintenance")]
        public class Remove : ModuleBase
        {
            [Command("owner")]
            [Remarks("Removes the specified user as one of my owners.")]
            public async Task RemoveOwner([Remainder]SocketGuildUser user)
            {
                Configuration config = Configuration.Load();
                if (user.Id == config.BotMaster)
                {
                    await ReplyAsync($"I love {user.Mention} too much to even consider it! :heart:");
                }
                else
                {
                    if (config.Owners.Contains(user.Id))
                    {
                        config.Owners.Remove(user.Id);
                        config.SaveJson();
                        await ReplyAsync($"{user.Mention} is no longer an owner. :v:");
                    }
                    else
                    {
                        await ReplyAsync($"{user.Mention} wasn't an owner in the first place, silly. :stuck_out_tongue_winking_eye: ");
                    }
                }
            }
        }


    }
}
