using Discord_Bot_Tutorial.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tutorial.Game;

namespace Discord_Bot_Tutorial
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public const string databaseName = "FImonDB";

        public async Task RunAsync()
        {
            string jsonBotConfigString = "";

            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                jsonBotConfigString = await sr.ReadToEndAsync().ConfigureAwait(false);


            string jsonLevelExperienceConfig = "";
            using (FileStream fs = File.OpenRead("levelExperience.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                jsonLevelExperienceConfig = await sr.ReadToEndAsync().ConfigureAwait(false);

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(jsonBotConfigString);
            List<int> levelConfig = JObject.Parse(jsonLevelExperienceConfig)["experience"].Select(x => (int)x).ToList();


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

            Commands.RegisterCommands<FImonCommands>(); // Register new Commands
            Commands.RegisterCommands<TeamCommands>();

            var client = new MongoClient("mongodb+srv://live2020:live2020pass@cluster0.shomo.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");

            AbilityManager.SetCollection(client.GetDatabase(databaseName));
            AbilityManager.LoadAbilities();

            FImonManager.SetCollection(client.GetDatabase(databaseName));
            FImonManager.LoadFimons();

            TrainerManager.SetCollection(client.GetDatabase(databaseName));
            TrainerManager.LoadTrainers();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(object sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

    }
}
