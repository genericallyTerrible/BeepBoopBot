using BeepBoopBot.Attributes;
using BeepBoopBot.Preconditions;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [BotModule("Example")]
    [MinPermissions(BotAccessLevel.User)]
    public class Example : ModuleBase<ShardedCommandContext>
    {

        [BotCommand, Usage, Description, Aliases]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.BotOwner, RequiredPreconditions.RequireAnyPrecondition)]
        public async Task Test()
        {
            SocketUser user = Context.User;
            await ReplyAsync($"Don't worry {user.Mention}, I'm still working.");
        }

        [BotCommand, Usage, Description, Aliases]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.BotOwner, RequiredPreconditions.RequireAnyPrecondition)]
        public async Task Say0
        (
            [Parameter(0)]
            SocketTextChannel textChannel,
            [Parameter(1), Remainder]
            string message
        )
        {
            SocketUser user = Context.User;
            await textChannel.SendMessageAsync($"{user.Mention} wanted me to say, \"{message}\"");
        }

        [BotCommand, Usage, Description, Aliases]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.BotOwner, RequiredPreconditions.RequireAnyPrecondition)]
        public async Task Say1
        (
            [Parameter(0)]
            SocketUser recipient,
            [Parameter(1), Remainder]
            string message
        )
        {
            SocketUser sender = Context.User;
            SocketSelfUser bot = Context.Client.CurrentUser;
            if (bot.Id != recipient.Id)
            {
                RestDMChannel dm = await recipient.CreateDMChannelAsync();
                await dm.SendMessageAsync($"{sender.Mention} wanted me to say, \"{message}\"");
            }
            else
            {
                await ReplyAsync($"Hey {sender.Mention}, do I look like I talk to myself?");
            }
        }

        [BotCommand, Usage, Description, Aliases]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.BotOwner, RequiredPreconditions.RequireAnyPrecondition)]
        public async Task Say2
        (
            [Parameter(0), Remainder]
            string message
        )
        {
            SocketUser user = Context.User;
            await ReplyAsync($"{user.Mention} wanted me to say \"{message}\"");
        }

        [Group("set"), Name("Example")]
        [RequireContext(ContextType.Guild)]
        public class Set : ModuleBase
        {

            [BotCommand, Usage, Description, Aliases]
            [RequireUserPermission(Discord.GuildPermission.ManageNicknames)]
            [RequireBotPermission(Discord.GuildPermission.ManageNicknames)]
            public async Task Nick0
            (
                [Parameter(0)]
                SocketGuildUser user,
                [Parameter(1), Remainder]
                string name
            )
            {
                SocketGuildUser admin = Context.User as SocketGuildUser;
                await user.ModifyAsync(x => x.Nickname = name);
                await ReplyAsync($"{user.Mention}, {admin.Mention} changed your name to **{name}**");
            }

            [BotCommand, Usage, Description, Aliases]
            [RequireBotPermission(Discord.GuildPermission.ManageNicknames)]
            public async Task Nick1
            (
                [Parameter(0), Remainder]
                string name
            )
            {
                SocketGuildUser user = Context.User as SocketGuildUser;
                string oldNick = user.Nickname;
                await user.ModifyAsync(x => x.Nickname = name);
                await ReplyAsync($"{oldNick} changed their name to {user.Mention}");
            }

            [BotCommand, Usage, Description, Aliases]
            [MinPermissions(BotAccessLevel.BotOwner)]
            public async Task BotNick
            (
                [Parameter(0), Remainder]
                string name
            )
            {
                Discord.IGuildUser self = await Context.Guild.GetCurrentUserAsync();
                await self.ModifyAsync(x => x.Nickname = name);

                await ReplyAsync($"I changed my name to **{name}**");
            }

            [Group("me"), Name("Me")]
            [MinPermissions(BotAccessLevel.BotMaster)]
            public class Me : ModuleBase
            {
                [Command("trash")]
                [MinPermissions(BotAccessLevel.BotMaster)]
                public async Task Trash()
                {

                    await ReplyAsync("I am trash");
                }

                [Group("total"), Name("Example")]
                public class Example : ModuleBase
                {
                    [Command("trash")]
                    [MinPermissions(BotAccessLevel.BotMaster)]
                    public async Task Trash()
                    {

                        await ReplyAsync("I am trash");
                    }
                }
            }
        }
    }
}
