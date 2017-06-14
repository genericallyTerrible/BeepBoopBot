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

        private static readonly int DefaultLeftPadding = 15;
        private static readonly int AutostartDelay = 5000;

        static BeepBoopBot()
        {
        }

        public async Task StartAsync()
        {
            // Let the user know I'm awake!
            await Client_Log(ClassName, "Booting up!");

            Configuration.EnsureExists();                    // Ensure the configuration file has been created.

            Configuration config = Configuration.Load();     // Loads the current configuration.

            Configuration.PrintConfiguration(config);

            if (!config.AutoStart)
                config.PromptForChanges();                   // Asks the user if they want to change the bot's settings.

            ConsoleCountdown countdown = new ConsoleCountdown(AutostartDelay, "Booting up in ");
            countdown.CountdownCompleted += OnStartup;
            countdown.CountdownCanceled += OnStartup;
            countdown.StartCountdown();

            await Task.Delay(-1);                            // Prevent the console window from closing.
        }

        private void OnStartupCountdownCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("The countdown completed.");
        }

        private void OnStartupCountdownCanceled(object sender, EventArgs e)
        {
            Console.WriteLine("The countdown was canceled.");
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

            Client.Log += Client_Log;                        // Register the console log event.

            await Client.LoginAsync(TokenType.Bot, config.Token);
            await Client.StartAsync();

            CommandHandler = new CommandHandler();           // Initialize the command handler service
            await CommandHandler.InstallAsync(Client);
        }

        public static Task Client_Log(string source, string message, LogSeverity severity = LogSeverity.Info)
        {
            Client_Log(new LogMessage(severity, source, message), DefaultLeftPadding);
            return Task.CompletedTask;
        }

        public static Task Client_Log(LogMessage msg)
        {
            Client_Log(msg, DefaultLeftPadding);
            return Task.CompletedTask;
        }

        public static Task Client_Log(LogMessage msg, int leftPadding)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Out.WriteLineAsync(msg.ToString(null, true, true, DateTimeKind.Local, leftPadding));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Out.WriteLineAsync(msg.ToString(null, true, true, DateTimeKind.Local, leftPadding));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Out.WriteLineAsync(msg.ToString(null, true, true, DateTimeKind.Local, leftPadding));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogSeverity.Info:
                    Console.Out.WriteLineAsync(msg.ToString(null, true, true, DateTimeKind.Local, leftPadding));
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Out.WriteLineAsync(msg.ToString(null, true, true, DateTimeKind.Local, leftPadding));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Out.WriteLineAsync(msg.ToString(null, true, true, DateTimeKind.Local, leftPadding));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            return Task.CompletedTask;
        }

        public static Task Client_Log_Multiple(
            LogSeverity severity, string source, string[] sources = null, string[] messages = null, string[] ids = null,
            bool trimLeft = true, bool trimCenter = true, int leftWidth = 10, int centerWidth = 15
            )
        {
            int length = 0;

            if (sources?.Length > 0) { length = sources.Length; }
            if (messages?.Length > length) { length = messages.Length; }
            if (ids?.Length > length) { length = ids.Length; }

            for (int i = 0; i < length; i++)
            {
                // Such coalesce, much wow.
                // If the array is not null and has an entry at i, trim the entry if possible (not null), or return "".
                string left = sources?.Length > i ? (sources[i]?.Trim() ?? "") : "";
                string center = messages?.Length > i ? (messages[i]?.Trim() ?? "") : "";
                string right = ids?.Length > i ? (ids[i]?.Trim() ?? "") : "";

                // If the left section is too long
                if (trimLeft && left.Length >= leftWidth)
                {
                    // Trim the left section
                    left = left.Substring(0, leftWidth - 1);
                }
                // Pad the left section
                left = left.PadRight(leftWidth - 1);

                // If the center section is too long
                if (trimCenter && center.Length >= centerWidth)
                {
                    // Trim the center section
                    center = center.Substring(0, centerWidth - 1);
                }
                // Pad the center section
                center = center.PadRight(centerWidth - 1);

                //Log the final message
                Client_Log(new LogMessage
                    (
                        severity,
                        source,
                        left.ToUpperInvariant() +
                        (!string.IsNullOrWhiteSpace(center) ? (" | " + center) : "") +
                        (!string.IsNullOrWhiteSpace(right) ? ($"[{right}]") : "")
                    ));
            }

            return Task.CompletedTask;
        }
    }
}
