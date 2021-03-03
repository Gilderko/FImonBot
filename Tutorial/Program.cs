using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Discord_Bot_Tutorial
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
