using BeepBoopBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace BeepBoopBot
{
    public class BeepBoopBot
    {
        public static string ClassName { get; private set; } = typeof(BeepBoopBot).Name;

        public static CommandHandler CommandHandler { get; private set; }
        public static CommandService CommandService { get; private set; }
        public static DiscordShardedClient Client { get; private set; }

        public static Localization Localization { get; private set; }

        // Time, specified in ticks (ms), to delay bot exectuion after sucessful configuration loading.
        private static readonly int AutostartDelay = 5000;

        public static Configuration Config { get; private set; }
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

            Config = Configuration.Load();                   // Loads the current configuration.

            Logger.LoggingSeverity = Config.LoggingSeverity; // Sets the logger to only display the user defined logging events.

            Configuration.PrintConfiguration(Config);

            if (!Config.AutoStart)
            {
                Config = Config.PromptForChanges();          // Asks the user if they want to change the bot's settings.
            }
            // Gives a cancelable delay.
            ConsoleCountdown countdown = new ConsoleCountdown("Booting up in ", AutostartDelay);
            countdown.CountdownStopped += OnStartup;
            countdown.StartCountdown();

            await Task.Delay(-1);                            // Prevent the console window from closing.
        }

        private async void OnStartup(object sender, EventArgs e)
        {

            // Create a new instance of DiscordSocketClient.
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = Config.LoggingSeverity,           // Specify console verbosity level.
                //TotalShards = 
                MessageCacheSize = 1000                      // Tell discord.net how long to store messages (per channel).
            });

            Logger logger = new Logger(Config.LoggingSeverity);

            Client.Log += logger.Log;                        // Register the console log event.

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();

            CommandHandler = new CommandHandler();           // Initialize the command handler service
            await CommandHandler.InstallAsync(Client);
        }
    }
}
