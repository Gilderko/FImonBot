using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using FImonBot.Commands;
using FImonBot.Game;
using FImonBot.Game.Stats;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FImonBot
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
            var levelConfig = JObject.Parse(jsonLevelExperienceConfig)["experience"].ToObject<int[]>();
            BaseStats.InitialiseBaseStats(levelConfig);

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
                Timeout = TimeSpan.FromMinutes(2)
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
            // Commands.RegisterCommands<TeamCommands>();
            Commands.RegisterCommands<CombatCommands>();
            Commands.RegisterCommands<TrainerCommands>();
            Commands.RegisterCommands<AbilityCommands>();
            Commands.RegisterCommands<SharedBaseForCommands>();
            Commands.RegisterCommands<AdminCommands>();
            Commands.RegisterCommands<QuizCommands>();

            var client = new MongoClient("mongodb+srv://adminPokus:mypassword123@cluster0.shomo.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");

            Console.WriteLine(client.GetDatabase(databaseName));

            var database = client.GetDatabase(databaseName);

            AbilityManager.SetCollection(database);
            FImonManager.SetCollection(database);
            TrainerManager.SetCollection(database);
            QuizManager.SetCollection(database);

            // load quizes, abilities
            var quizesLoad = QuizManager.LoadQuestions();
            var abilityLoad = AbilityManager.LoadAbilities();
            var bansLoad = BanManager.LoadFile();

            Task.WaitAll(quizesLoad, abilityLoad, bansLoad);
            // load fimons
            await FImonManager.LoadFimons();
            // load trainers
            await TrainerManager.LoadTrainers();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(object sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

    }
}
