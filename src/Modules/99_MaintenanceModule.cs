using Discord;
using Discord.Commands;
using Discord.WebSocket;
using BeepBoopBot.Preconditions;
using System.Threading.Tasks;
using System;

namespace BeepBoopBot.Modules
{
    [Name("Bot Maintenance")]
    [MinPermissions(BotAccessLevel.BotOwner)]
    public class MaintenanceModule : ModuleBase<ShardedCommandContext>
    {
        [Command("config")]
        [Remarks("Lists the bot's current configuration")]
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

        [MinPermissions(BotAccessLevel.BotMaster)]
        [Group("add"), Name("Bot Maintenance")]
        public class Add : ModuleBase
        {
            [Command("owner", RunMode = RunMode.Async)]
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

        [MinPermissions(BotAccessLevel.BotMaster)]
        [Group("remove"), Name("Bot Maintenance")]
        public class Remove : ModuleBase
        {
            [Command("owner", RunMode = RunMode.Async)]
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

        [Group("modify"), Name("Bot Maintenance")]
        public class Modify : ModuleBase
        {
            [Command("color")]
            [Remarks("")]
            public async Task ModifyColor
            (
            [Summary("Integer value of the red component of the new color.")]
            int r,
            [Summary("Integer value of the green component of the new color.")]
            int g,
            [Summary("Integer value of the blue component of the new color.")]
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

            [Command("color")]
            [Remarks("")]
            public async Task ModifyColor
                (
                [Summary("The hex code of the new color."), Remainder]string hexColor
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
