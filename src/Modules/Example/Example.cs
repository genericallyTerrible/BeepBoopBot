using BeepBoopBot.Attributes;
using BeepBoopBot.Extensions;
using BeepBoopBot.Preconditions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules
{
    [BotModule("Example")]
    [MinPermissions(BotAccessLevel.User)]
    public class Example : ModuleBase<ShardedCommandContext>
    {

        [Command("modify embed color")]
        [MinPermissions(BotAccessLevel.BotMaster)]
        public async Task Testing
        (
            ulong messageId,
            [Remainder] string hexColor
        )
        {
            Configuration config = Configuration.Load();
            Color newColor;
            if (hexColor[0] == '#') //Trim hastags if necessary
                hexColor = hexColor.Substring(1);

            if (hexColor.ToLower().Equals("default"))
            {
                newColor = Configuration.DefaultEmbedColor;
            }
            else if (hexColor.Length < 7)
            {
                uint newVal = uint.Parse(hexColor, System.Globalization.NumberStyles.HexNumber);
                if (newVal <= new Color(255, 255, 255).RawValue)
                {
                    newColor = new Color(newVal);
                }
                else
                {
                    throw new OverflowException();
                }
            }
            else
            {
                throw new FormatException("Input string was not in a correct format.");
            }

            SocketUserMessage message = (SocketUserMessage)(await Context.Channel.GetMessageAsync(messageId));
            if (message != null)
            {
                if (message.Embeds.Count > 0)
                {
                    IEmbed oldEmbed = message.Embeds.ToList()[0];
                    EmbedBuilder newEmbed = oldEmbed.ToEmbedBuilder();
                    if (newEmbed != null)
                    {
                        newEmbed.Color = newColor;
                        await message.ModifyAsync(m => m.Embed = newEmbed.Build());
                    }
                    else
                    {
                        await ReplyAsync("I'm sorry, the message you specified had no modifiable embeds.");
                    }
                }
                else
                {
                    await ReplyAsync("I'm sorry, the message you specified had no embeds.");
                }
            }
            else
            {
                await ReplyAsync("I'm sorry, I couldn't find the message you were after.");
            }
        }
        
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
                IDMChannel dm = await recipient.GetOrCreateDMChannelAsync();
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
        }
    }
}
