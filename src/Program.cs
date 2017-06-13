using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace BeepBoopBot
{
    public class Program
    {
        public static void Main(string[] args)
            => new BeepBoopBot().StartAsync().GetAwaiter().GetResult();
    }
}
