using Discord.Commands;
using Discord.WebSocket;
using BeepBoopBot.Preconditions;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [Name("Example")]
    [MinPermissions(AccessLevel.User)]
    public class ExampleModule : ModuleBase<SocketCommandContext>
    {
        [Command("say"), Alias("s")]
        [Remarks("Make the bot say something")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task Say([Remainder]string text)
        {
            await ReplyAsync(text);
        }

        [Group("set"), Name("Example")]
        public class Set : ModuleBase
        {

            [Command("nick")]
            [Remarks("Makes the bot change the user's nickname")]
            [MinPermissions(AccessLevel.ServerAdmin)]
            public async Task Nick(SocketGuildUser user, [Remainder] string name)
            {
                SocketGuildUser admin = Context.User as SocketGuildUser;
                await user.ModifyAsync(x => x.Nickname = name);
                await ReplyAsync($"{user.Mention}, {admin.Mention} changed your name to **{name}**");
            }

            [Command("nick")]
            [Remarks("Makes the bot change the user's nickname")]
            public async Task Nick([Remainder]string name)
            {
                SocketGuildUser user = Context.User as SocketGuildUser;
                await user.ModifyAsync(x => x.Nickname = name);
                await ReplyAsync($"{user.Mention}, I changed your name to **{name}**");
            }

            [Command("botnick")]
            [Remarks("Changes the bot's nickname")]
            [MinPermissions(AccessLevel.ServerOwner)]
            public async Task BotNick([Remainder]string name)
            {
                Discord.IGuildUser self = await Context.Guild.GetCurrentUserAsync();
                await self.ModifyAsync(x => x.Nickname = name);

                await ReplyAsync($"I changed my name to **{name}**");
            }

            [Group("me"), Name("Example")]
            public class Me : ModuleBase
            {
                [Command("trash")]
                [MinPermissions(AccessLevel.BotMaster)]
                public async Task Trash()
                {

                    await ReplyAsync("I am trash");
                }
            }
        }
    }
}
