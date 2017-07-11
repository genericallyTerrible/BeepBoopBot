using BeepBoopBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeepBoopBot
{
    public class BeepBoopBot
    {
        public static string ClassName { get; private set; } = typeof(BeepBoopBot).Name;

        public static List<SocketTextChannel> UpdateChannels { get; set; } = new List<SocketTextChannel>();

        public static CommandHandler CommandHandler { get; private set; }
        public static CommandService CommandService { get; private set; }
        public static DiscordShardedClient Client { get; private set; }

        public static Localization Localization { get; private set; }

        // Time, specified in ticks (ms), to delay bot exectuion after sucessful configuration loading.
        private static readonly int AutostartDelay = 5000;

        //public static Configuration Config { get; private set; }
        public static Logger Logger { get; private set; }

        static BeepBoopBot()
        {
            Logger = new Logger();                           // Initializes a new logger that logs everything.
        }

        public async Task StartAsync()
        {
            // Let the user know I'm awake!
            await Logger.Log(ClassName, "Booting up!");

            Configuration.EnsureExists();                    // Ensure the configuration file has been created.

            Configuration config = Configuration.Load();                   // Loads the current configuration.

            Logger.LoggingSeverity = config.LoggingSeverity; // Sets the logger to only display the user defined logging events.

            Configuration.PrintConfiguration(config);

            if (!config.AutoStart)
            {
                config = config.PromptForChanges();          // Asks the user if they want to change the bot's settings.
            }
            // Gives a cancelable delay.
            ConsoleCountdown countdown = new ConsoleCountdown("Booting up in ", AutostartDelay);
            countdown.CountdownStopped += OnStartup;
            countdown.StartCountdown();

            await Task.Delay(-1);                            // Prevent the console window from closing.
        }

        private async void OnStartup(object sender, EventArgs e)
        {
            Configuration config = Configuration.Load();
            // Create a new instance of DiscordSocketClient.
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = config.LoggingSeverity,           // Specify console verbosity level.
                //TotalShards = 
                MessageCacheSize = 1000                      // Tell discord.net how long to store messages (per channel).
            });

            Logger logger = new Logger(config.LoggingSeverity);

            Client.Log += logger.Log;                        // Register the console log event.

            await Client.LoginAsync(TokenType.Bot, config.Token);
            await Client.StartAsync();

            CommandHandler = new CommandHandler();           // Initialize the command handler service
            await CommandHandler.InstallAsync(Client);
        }
    }
}
