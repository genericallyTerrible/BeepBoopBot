using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BeepBoopBot.Services
{
    /// <summary> Detect whether a message is a command, then execute it. </summary>
    public class CommandHandler
    {
        public static string ClassName { get; private set; } = typeof(CommandHandler).Name;

        private DiscordShardedClient Client;
        private CommandService CommandService;

        public async Task InstallAsync(DiscordShardedClient c)
        {
            Client = c;                                                 // Save an instance of the discord client.
            CommandService = new CommandService();                                // Create a new instance of the commandservice.                              

            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());    // Load all modules from the assembly.

            Client.MessageReceived += HandleCommandAsync;               // Register the messagereceived event to handle commands.
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            SocketUserMessage msg = s as SocketUserMessage;
            if (msg == null)                                          // Check if the received message is from a user.
                return;

            ShardedCommandContext context = new ShardedCommandContext(Client, msg);     // Create a new command context.

            int argPos = 0;                                           // Check if the message is not from a bot and has either a string or mention prefix or is a DM.
            if (
                !msg.Author.IsBot &&
                (
                    msg.HasStringPrefix(Configuration.Load().Prefix, ref argPos) ||
                    msg.HasMentionPrefix(Client.CurrentUser, ref argPos) ||
                    context.IsPrivate
                ))
            {                                                         // Try and execute a command with the given context.
                try
                {
                    IResult result = await CommandService.ExecuteAsync(context, argPos);
                    if (!result.IsSuccess)                                // If execution failed, reply with the error message.
                    {
                        await context.Channel.SendMessageAsync($"Oops, I hit a snag: {result.ErrorReason}");
                        await LogError(msg, result);
                    }
                    else
                    {
                        await LogSuccess(msg);
                    }
                }
                catch (Exception e)
                {
                    await context.Channel.SendMessageAsync($"Uh oh, something went wrong. . .\n{e.ToString()}");
                    await LogException(msg, e);
                }
            }
        }

        private static readonly string DMString = "Direct message";

        private async Task LogSuccess(IUserMessage msg)
        {
            LogSeverity severity = LogSeverity.Info;

            var baseLog = BuildBaseLog(msg);

            string[] sources = baseLog.Item1;
            string[] messages = baseLog.Item2;
            string[] ids = baseLog.Item3;

            await BeepBoopBot.Logger.LogMultiple(severity, ClassName, sources, messages, ids);
            // Log the message seperately to avoid trimming it.
            await BeepBoopBot.Logger.LogMultiple(severity, ClassName, new[] { "Message" }, new[] { msg.Content }, trimCenter: false);
        }

        private async Task LogError(IUserMessage msg, IResult result)
        {
            LogSeverity severity = LogSeverity.Error;

            var baseLog = BuildBaseLog(msg);

            string[] sources = baseLog.Item1;
            string[] messages = baseLog.Item2;
            string[] ids = baseLog.Item3;

            await BeepBoopBot.Logger.LogMultiple(severity, ClassName, sources, messages, ids);
            // Log the message seperately to avoid trimming it.
            await BeepBoopBot.Logger.LogMultiple(severity, ClassName, new[] { "Message" }, new[] { msg.Content }, trimCenter: false);
            // Log the error seperatley to avoid trimming it.
            await BeepBoopBot.Logger.LogMultiple(severity, ClassName, new[] { "Error" }, new[] { result.ErrorReason }, trimCenter: false);
        }

        private async Task LogException(IUserMessage msg, Exception e)
        {
            LogSeverity severity = LogSeverity.Critical;

            var baseLog = BuildBaseLog(msg);

            string[] sources  = baseLog.Item1;
            string[] messages = baseLog.Item2;
            string[] ids      = baseLog.Item3;
            
            await BeepBoopBot.Logger.LogMultiple(severity, ClassName, sources, messages, ids);
            // Log the message seperately to avoid trimming it.
            await BeepBoopBot.Logger.LogMultiple(severity, ClassName, new[] { "Message" }, new[] { msg.Content }, trimCenter: false);
            // Log the error seperatley to avoid trimming it.
            await BeepBoopBot.Logger.LogMultiple(severity, ClassName, new[] { "Exception" }, new[] { e.Message }, trimCenter: false);
        }

        private Tuple<string[], string[], string[]> BuildBaseLog(IUserMessage msg)
        {
            bool isDM = (msg.Channel is SocketDMChannel);

            string[] sources = new string[3];
            string[] messages = new string[3];
            string[] ids = new string[3];

            sources[0] = "User";
            messages[0] = $"{msg.Author}";
            ids[0] = $"{msg.Author.Id}";

            sources[1] = "Server";
            if (isDM)
            {
                messages[1] = DMString;
            }
            else
            {
                messages[1] = ((IGuildChannel)msg.Channel).Guild.Name;
                ids[1] = $"{((IGuildChannel)msg.Channel).GuildId}";
            }

            sources[2] = "Channel";
            if (isDM)
            {
                messages[2] = DMString;
            }
            else
            {
                messages[2] = msg.Channel.Name;
                ids[2] = $"{msg.Channel.Id}";
            }

            return new Tuple<string[], string[], string[]>(sources, messages, ids);
        }
    }
}
