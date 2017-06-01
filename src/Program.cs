using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace BeepBoopBot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandHandler _commands;

        public async Task StartAsync()
        {
            Console.WriteLine("Booting up!");                // Let the user know I'm awake!

            Configuration.EnsureExists();                    // Ensure the configuration file has been created.

            Configuration config = Configuration.Load();     // Loads the current configuration.
            Console.WriteLine(Configuration.ListConfiguration(config));


            if (!config.AutoStart)
                config.PromptForChanges();                   // Asks the user if they want to change the bot's settings.

                                                             // Create a new instance of DiscordSocketClient.
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,              // Specify console verbose information level.
                MessageCacheSize = 1000                      // Tell discord.net how long to store messages (per channel).
            });

            _client.Log += (l)                               // Register the console log event.
                => Console.Out.WriteLineAsync(l.ToString());
                                   
            await _client.LoginAsync(TokenType.Bot, config.Token);
            await _client.StartAsync();

            _commands = new CommandHandler();                // Initialize the command handler service
            await _commands.InstallAsync(_client);
            
            await Task.Delay(-1);                            // Prevent the console window from closing.
        }
    }
}
