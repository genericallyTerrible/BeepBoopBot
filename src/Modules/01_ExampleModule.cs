using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using BeepBoopBot.Preconditions;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [Name("Example")]
    [MinPermissions(BotAccessLevel.User)]
    public class ExampleModule : ModuleBase<SocketCommandContext>
    {

        [Command("test")]
        [Remarks("See if the bot's still working")]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.BotOwner, RequiredPreconditions.RequireAnyPrecondition)]
        public async Task Test()
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            await ReplyAsync($"Don't worry {user.Mention}, I'm still working.");
        }

        [Command("say")]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.BotOwner, RequiredPreconditions.RequireAnyPrecondition)]
        public async Task Say(SocketTextChannel textChannel, [Remainder] string message)
        {
            SocketUser user = Context.User;
            await textChannel.SendMessageAsync($"{user.Mention} wanted me to say, \"{message}\"");
        }

        [Command("say")]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.BotOwner, RequiredPreconditions.RequireAnyPrecondition)]
        public async Task Say(SocketUser recipient, [Remainder] string message)
        {
            SocketUser sender = Context.User;
            RestDMChannel dm = await recipient.CreateDMChannelAsync();
            await dm.SendMessageAsync($"{sender.Mention} wanted me to say, \"{message}\"");
        }

        [Command("say")]
        [Alias("s")]
        [Remarks("Make the bot say something")]
        [MinPermissions(ServerAccessLevel.ServerAdmin, BotAccessLevel.BotOwner, RequiredPreconditions.RequireAnyPrecondition)]
        public async Task Say([Remainder]string message)
        {
            SocketUser user = Context.User;
            await ReplyAsync($"{user.Mention} wanted me to say \"{message}\"");
        }

        [Group("set"), Name("Example")]
        public class Set : ModuleBase
        {

            [Command("nick")]
            [Remarks("I change the nickname of whoever you mentioned to whatever you wanted")]
            [RequireUserPermission(Discord.GuildPermission.ManageNicknames)]
            [RequireBotPermission(Discord.GuildPermission.ManageNicknames)]
            [Priority(1)]
            public async Task Nick(SocketGuildUser user, [Remainder] string name)
            {
                SocketGuildUser admin = Context.User as SocketGuildUser;
                await user.ModifyAsync(x => x.Nickname = name);
                await ReplyAsync($"{user.Mention}, {admin.Mention} changed your name to **{name}**");
            }

            [Command("nick")]
            [Remarks("Lets me change your nickname to whatever you want.")]
            [RequireBotPermission(Discord.GuildPermission.ManageNicknames)]
            [Priority(0)]
            public async Task Nick([Remainder]string name)
            {
                SocketGuildUser user = Context.User as SocketGuildUser;
                await user.ModifyAsync(x => x.Nickname = name);
                await ReplyAsync($"{user.Mention}, I changed your name to **{name}**");
            }

            [Command("botnick")]
            [Remarks("Changes the bot's nickname")]
            [MinPermissions(BotAccessLevel.BotOwner)]
            [RequireContext(ContextType.Guild)]
            public async Task BotNick([Remainder]string name)
            {
                Discord.IGuildUser self = await Context.Guild.GetCurrentUserAsync();
                await self.ModifyAsync(x => x.Nickname = name);

                await ReplyAsync($"I changed my name to **{name}**");
            }

            [Group("me"), Name("Example")]
            [MinPermissions(BotAccessLevel.BotMaster)]
            public class Me : ModuleBase
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
