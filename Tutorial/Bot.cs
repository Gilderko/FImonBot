using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord_Bot_Tutorial.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Tutorial.FimonManager;

namespace Discord_Bot_Tutorial
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            string json = "";

            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,                
            };
            
            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            InteractivityConfiguration interactivityConfig = new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(3)
            };

            Client.UseInteractivity(interactivityConfig);
            
            CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = false,
                CaseSensitive = false,
                DmHelp = true,
                IgnoreExtraArguments = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            
            Commands.RegisterCommands<FunCommands>(); // Register new Commands
            Commands.RegisterCommands<TeamCommands>();

            var client = new MongoClient("mongodb+srv://live2020:live2020pass@cluster0.shomo.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
            Console.WriteLine(client);

            FimonManager.SetDatabase(client);
            FimonManager.LoadFimons();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(object sender,ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

    }
}
