using BeepBoopBot.Preconditions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace BeepBoopBot.Modules.UserInfo
{
    [Name("User Data")]
    [RequireContext(ContextType.Guild)]
    public class UserInfo : ModuleBase<ShardedCommandContext>
    {
        [Command("joindate")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(ServerAccessLevel.User)]
        public async Task JoinDate0()
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            await JoinDate1(user);
        }

        [Command("joindate")]
        [RequireContext(ContextType.Guild)]
        public async Task JoinDate1(SocketGuildUser user)
        {
            DateTimeOffset joinDate = user.JoinedAt ?? DateTimeOffset.MinValue;
            if (joinDate != DateTimeOffset.MinValue)
            {
                TimeSpan ts = DateTime.Now - joinDate.DateTime;
                string plural = ts.Days == 1 ? "" : "s";
                await ReplyAsync($"{user.Mention} joined {ts.Days} day{plural} ago on: {joinDate.ToString("MMMM dd, yyyy")}");
            }
            else
            {
                await ReplyAsync($"I'm sorry {user.Mention}, something went wrong.");
            }
        }

        //[Command("postfreq")]
        //[RequireContext(ContextType.Guild)]
        //public async Task PostFrequency(SocketGuildUser user)
        //{
        //    SocketGuild guild = Context.Guild;
            
        //}
    }
}
