using System;

namespace FImonBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bot bot = new Bot();

            Console.WriteLine("Got here");
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
