using Discord;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BeepBoopBot.Services
{
    public class Logger
    {
        public ConsoleColor DefaultForeground { get; private set; }
        public ConsoleColor DefaultBackground { get; private set; }

        public int DefaultLeftPadding { get; private set; }
        public LogSeverity LoggingSeverity { get; set; }

        private const int defaultPadding = 15;

        public Logger(int defaultLeftPadding = defaultPadding)
        {
            DefaultLeftPadding = defaultLeftPadding;
            LoggingSeverity = LogSeverity.Debug;                // Log everything if not specified.
            DefaultForeground = Console.ForegroundColor;
            DefaultBackground = Console.BackgroundColor;
        }

        public Logger(LogSeverity loggingSeverity, int defaultLeftPadding = defaultPadding)
        {
            DefaultLeftPadding = defaultLeftPadding;
            LoggingSeverity = loggingSeverity;
            DefaultForeground = Console.ForegroundColor;
            DefaultBackground = Console.BackgroundColor;
        }

        /// <summary>
        /// Creates a new LogMessage and writes it to the console.
        /// </summary>
        /// <param name="source">The source of the LogMessage.</param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="messageSeverity">The severity of the message.</param>
        /// <returns></returns>
        public Task Log(string source, string message, LogSeverity messageSeverity = LogSeverity.Info)
        {
            Log(new LogMessage(messageSeverity, source, message), DefaultLeftPadding);
            return Task.CompletedTask;
        }

        public Task Log(LogMessage msg)
        {
            Log(msg, DefaultLeftPadding);
            return Task.CompletedTask;
        }

        public Task Log(LogMessage msg, int leftPadding)
        {
            if (msg.Severity <= LoggingSeverity)
            {
                switch (msg.Severity)
                {
                    case LogSeverity.Critical:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(msg.ToString(padSource: leftPadding));
                        break;
                    case LogSeverity.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(msg.ToString(padSource: leftPadding));
                        break;
                    case LogSeverity.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(msg.ToString(padSource: leftPadding));
                        break;
                    case LogSeverity.Info:
                        Console.WriteLine(msg.ToString(padSource: leftPadding));
                        break;
                    case LogSeverity.Verbose:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(msg.ToString(padSource: leftPadding));
                        break;
                    case LogSeverity.Debug:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(msg.ToString(padSource: leftPadding));
                        break;
                }
                // Reset console colors
                Console.BackgroundColor = DefaultBackground;
                Console.ForegroundColor = DefaultForeground;
            }

            return Task.CompletedTask;
        }

        public Task LogMultiple(
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
                Log(new LogMessage
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
