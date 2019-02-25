using BeepBoopBot.Attributes;
using BeepBoopBot.Preconditions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;

namespace BeepBoopBot.Modules
{
    [BotModule("Maintenance")]
    [MinPermissions(BotAccessLevel.BotOwner)]
    public class Maintenance : ModuleBase<ShardedCommandContext>
    {
        [BotCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.DM)]
        public async Task Config()
        {
            Configuration config = Configuration.Load();
            EmbedBuilder settingsBuilder = new EmbedBuilder()
            {
                Color = config.EmbedColor
            };

            settingsBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = "Prefix",
                Value = config.Prefix
            });
            settingsBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = "Bot Master",
                Value = $"<@{config.BotMaster}>"
            });
            settingsBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = "Embed Color",
                Value = config.EmbedColor.ToString().ToUpper()
            });
            settingsBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = "Autostart",
                Value = config.AutoStart ? "Enabled" : "Disabled"
            });
            settingsBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = "Logging severity",
                Value = config.LoggingSeverity.ToString()
            });

            await ReplyAsync("", false, settingsBuilder.Build());
        }

        [Command("servers")]
        [RequireContext(ContextType.DM)]
        public async Task Servers()
        {
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = Configuration.Load().EmbedColor,
                Description = "Here's a list of servers I'm connected to."
            };

            List<SocketGuild> guilds = new List<SocketGuild>(BeepBoopBot.Client.Guilds);
            foreach (SocketGuild guild in guilds)
            {
                EmbedFieldBuilder guildEmbed = new EmbedFieldBuilder()
                {
                    Name = guild.Name,
                    Value = $"ID: {guild.Id}"
                };
                embed.AddField(guildEmbed);
            }

            if(embed.Fields.Count > 0)
            {
                await ReplyAsync("", embed: embed);
            }
            else
            {
                await ReplyAsync("I'm currently not connected to any servers.");
            }
        }

        [Command("leave")]
        [RequireContext(ContextType.DM)]
        [MinPermissions(BotAccessLevel.BotMaster)]
        public async Task Leave(ulong guildId)
        {
            List<SocketGuild> guilds = new List<SocketGuild>(BeepBoopBot.Client.Guilds);
            int index = guilds.FindIndex(g => g.Id == guildId);
            if(index >= 0)
            {
                // Guild was found in the list.
                await guilds[index].LeaveAsync();
                await ReplyAsync($"I have successfully left {guilds[index].Name}");
            }
            else
            {
                // Guild was not found in the list.
                await ReplyAsync("I'm sorry, I am not currently connected to any guild with that Id");
            }
        }

        //[Command("die")]
        //[MinPermissions(BotAccessLevel.BotMaster)]
        //public async Task Die()
        //{
        //    await BeepBoopBot.Client.StopAsync();
        //}

        [MinPermissions(BotAccessLevel.BotMaster)]
        [Group("add"), Name("Maintenance")]
        public class Add : ModuleBase
        {
            [BotCommand, Usage, Description, Aliases]
            public async Task AddOwner
            (
                [Parameter(0), Remainder]
                SocketGuildUser user
            )
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

        [MinPermissions(BotAccessLevel.BotMaster)]
        [Group("remove"), Name("Maintenance")]
        public class Remove : ModuleBase
        {
            [BotCommand, Usage, Description, Aliases]
            public async Task RemoveOwner
            (
                [Parameter(0), Remainder]
                SocketGuildUser user
            )
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

        [Group("modify"), Name("Maintenance")]
        public class Modify : ModuleBase
        {
            [BotCommand, Usage, Description, Aliases]
            public async Task ModifyColor0
            (
                [Parameter(0)]
                int r,
                [Parameter(0)]
                int g,
                [Parameter(0)]
                int b
            )
            {
                Configuration config = Configuration.Load();
                config.EmbedColor = new Color(r, g, b);
                config.SaveJson();
                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Color = config.EmbedColor,
                    Description = "This is what my new embed color looks like!"
                };

                await ReplyAsync("", false, embedBuilder.Build());

            }

            [BotCommand, Usage, Description, Alias]
            public async Task ModifyColor1
            (
                [Parameter(0), Remainder]
                string hexColor
            )
            {
                Configuration config = Configuration.Load();
                bool success = false;

                if (hexColor[0] == '#') //Trim hastags if necessary
                    hexColor = hexColor.Substring(1);

                if (hexColor.ToLower().Equals("default"))
                {
                    config.EmbedColor = Configuration.DefaultEmbedColor;
                    config.SaveJson();
                    success = true;
                }
                else if (hexColor.Length < 7)
                {
                    uint newVal = uint.Parse(hexColor, System.Globalization.NumberStyles.HexNumber);
                    if (newVal <= new Color(255, 255, 255).RawValue)
                    {
                        config.EmbedColor = new Color(newVal);
                        config.SaveJson();
                        success = true;
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

                if (success)
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                    {
                        Color = config.EmbedColor,
                        Description = "This is what my new embed color looks like!"
                    };
                    await ReplyAsync("", false, embedBuilder.Build());
                }
            }
        }
    }
}
