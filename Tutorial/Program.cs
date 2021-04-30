using System;

namespace FImonBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();

            Console.WriteLine("Got here");
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
