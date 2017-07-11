using BeepBoopBot.Preconditions;
using BeepBoopBot.Services.Database.Models;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules.Webcomic
{
    [Name("Webcomic")]
    public class Webcomic : ModuleBase<ShardedCommandContext>
    {
        public bool Updating { get; private set; } = false;

        [Command("add update")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.ComicAuthor)]
        public async Task AddUpdateChannel()
        {
            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            BeepBoopBot.UpdateChannels.Add(channel);
            await ReplyAsync($"{channel.Guild.Name}'s {channel.Mention} has been added as an update channel.");
        }

        [Command("add update")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.ComicAuthor)]
        public async Task AddUpdateChannel(SocketTextChannel channel)
        {
            BeepBoopBot.UpdateChannels.Add(channel);
            await ReplyAsync($"{channel.Guild.Name}'s {channel.Mention} has been added as an update channel.");
            DbWebcomic dbwc = new DbWebcomic();
        }

        [Command("list update")]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.ComicAuthor)]
        public async Task ListUpdateChannels()
        {
            string response = string.Join(", ", BeepBoopBot.UpdateChannels);
            await ReplyAsync(response);

        }

        [Command("update")]
        [RequireContext(ContextType.DM)]
        [MinPermissions(BotAccessLevel.ComicAuthor)]
        public async Task Update()
        {
            foreach (SocketTextChannel channel in BeepBoopBot.UpdateChannels)
            {
                await channel.SendMessageAsync("Update successful.");
            }
        }

        [Command("updating")]
        [RequireContext(ContextType.DM)]
        [MinPermissions(BotAccessLevel.ComicAuthor)]
        public async Task StartUpdating()
        {
            Updating = true;
        }

    }

    public class Updating
    {

    }
}
