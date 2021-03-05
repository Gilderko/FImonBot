using Discord_Bot_Tutorial.Attributes;
using Discord_Bot_Tutorial.Handlers.Dialogue;
using Discord_Bot_Tutorial.Handlers.Dialogue.Steps;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tutorial.FimonManager;

namespace Discord_Bot_Tutorial.Commands
{
    public class FunCommands : BaseCommandModule
    {
        [Command("ping")] // The actual string they have to type to trigger it
        [Description("Returns pong")]
        [RequireCategoriesAttribute(ChannelCheckMode.Any, "Text Channels")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false); // Await means it wont continue until it is done
        }

        [Command("add")]
        [Description("Adds two numbers together")]
        public async Task Add(CommandContext ctx, [Description("num1")] int numberOne, [Description("num2")] int numberTwo)
        {
            await ctx.Channel.SendMessageAsync((numberOne+numberTwo).ToString()).ConfigureAwait(false);
            await ctx.Member.SendMessageAsync("Oi cunt").ConfigureAwait(false); 
        }

        [Command("greet")]
        [Description("Greets a person in DM")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task Greet(CommandContext ctx)
        {
            DiscordMessage message = await ctx.Member.SendMessageAsync("Oi cunt").ConfigureAwait(false);            
            await Task.Delay(3000);
            await message.DeleteAsync();
        }

        [Command("respondMessage")]
        public async Task RespondMessage(CommandContext ctx)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            // X is the message that it is finding and checks if the message is correct based on the predicate
            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User).ConfigureAwait(false);
            if (!message.TimedOut)
            {
                await ctx.Channel.SendMessageAsync(message.Result.Content);
            }  
        }

        [Command("respondReaction")]
        public async Task RespondEmoji(CommandContext ctx)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            // X is the message that it is finding and checks if the message is correct based on the predicate
            var reaction = await interactivity.WaitForReactionAsync(x => x.Message
            == ctx.Message && x.Channel == ctx.Channel && x.User == ctx.User).ConfigureAwait(false);
            if (!reaction.TimedOut)
            {
                await ctx.Channel.SendMessageAsync(reaction.Result.Emoji);
            }
        }

        [Command("poll")]
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] emojiOptions)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            var options = emojiOptions.Select(x => x.ToString());

            var pollEmbed = new DiscordEmbedBuilder()
            {
                Title = "Poll",
                Color = DiscordColor.HotPink,
                Description = string.Join(" ", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            // Collecting reactions from pollMessage
            var result = await pollMessage.CollectReactionsAsync(duration).ConfigureAwait(false);
            
            Dictionary<DiscordEmoji, int> emojiCounts = new Dictionary<DiscordEmoji, int>();
            
            foreach (Reaction react in result)
            {
                var emoji = react.Emoji;

                if (emojiCounts.ContainsKey(emoji))
                {
                    emojiCounts[emoji] += 1;
                }
                else
                {
                    emojiCounts[emoji] = 1;
                }
            }

            IEnumerable<string> message = emojiCounts.Select(x => x.Key.Name.ToString() + " " + x.Value.ToString());

            await ctx.Channel.SendMessageAsync(string.Join("\n", message)).ConfigureAwait(false);
        }

        [Command("dialogue")]
        public async Task Dialogue(CommandContext ctx)
        {
            var inputStep = new TextStep("Enter something interesting!", null, 10);
            int low = 1;
            int high = 10;
            var funnyStep = new IntStep("Haha funny... add some fun number", null, low, high);

            string input = string.Empty;
            int value = 0;

            inputStep.OnValidResult += (result) => 
            {
                input = result;
                if (result == "hello there")
                {
                    inputStep.SetNextStep(funnyStep);
                }
            };

            funnyStep.OnValidResult += (result) => value = result;

            var userChannel = ctx.Channel;

            var inputDialogueHandler = new DialogueHandler(
                ctx.Client,
                userChannel,
                ctx.User,
                inputStep
            );

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);            

            if (!succeeded) { return; }

            await ctx.Channel.SendMessageAsync(input).ConfigureAwait(false);
        }

        [Command("emojiDialogue")]
        public async Task EmojiDialogue(CommandContext ctx)
        {
            var yesStep = new TextStep("You chose yes", null);
            var noStep = new TextStep("You chose no", null);

            var emojiStep = new ReactionStep("Yer or No?", new Dictionary<DiscordEmoji, ReactionStepData> 
            {
                { DiscordEmoji.FromName(ctx.Client,":thumbsup:"), new ReactionStepData{ Content = "This means yes", NextStep = yesStep } },
                { DiscordEmoji.FromName(ctx.Client,":thumbsdown:"), new ReactionStepData{ Content = "This means no" , NextStep = noStep } }
            });

            var userChannel = ctx.Channel;

            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User,emojiStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }
        }

        [Command("addfimon")]
        public async Task AddFimonCommand(CommandContext ctx)
        {
            var userID = ctx.User.Id;
            if (FimonManager.mapping.ContainsKey(userID))
            {
                await ctx.Channel.SendMessageAsync("Mate... you already have a FImon").ConfigureAwait(false);
                return;
            }

            var nameStep = new TextStep("Welcome \nPlease choose the name for your FImon", null, 1, 30);
            string FImonName = "";
            nameStep.OnValidResult += (result) => FImonName = result;

            var descriptionStep = new TextStep("Great... Now write a short description for your FImon", null, 10, 200);
            string description = "";
            descriptionStep.OnValidResult += (result) => description = result;
            Dictionary<DiscordEmoji, ReactionStepData> reactionTypeOptions = GetFImonTypesReactionOptions(ctx);

            var primaryTypeStep = new ReactionStep("Now what is your primary type?", reactionTypeOptions);
            FImonType primaryType = FImonType.Air;
            primaryTypeStep.OnValidResult += (result) =>
            {
                primaryType = (FImonType)primaryTypeStep.GetOptions()[result].optionalData;
                reactionTypeOptions.Remove(result);
            };

            var secondaryTypeStep = new ReactionStep("Now what is your secondary type?", reactionTypeOptions);
            FImonType secondaryType = FImonType.Air;
            secondaryTypeStep.OnValidResult += (result) =>
            {
                secondaryType = (FImonType)secondaryTypeStep.GetOptions()[result].optionalData;
                reactionTypeOptions.Remove(result);
            };

            DiscordEmbedBuilder attributesIntroEmbed = AttributesEmbedInfo();

            var introToAttributes = new ReactionStep("", new Dictionary<DiscordEmoji, ReactionStepData>
            {
                { DiscordEmoji.FromName(ctx.Client,":thumbsup:"), new ReactionStepData{ Content = "Continue" } }
            });
            introToAttributes.optionalEmbed = attributesIntroEmbed;

            int pointsToUse = 30;
            int lowBound = 1;
            int highBound = 10;

            var strengthStep = new IntStep("Please state a value of you strength", null, lowBound, highBound);
            int strengthValue = 1;
            strengthStep.OnValidResult += (result) =>
            {
                strengthValue = result;
                pointsToUse -= result;
                ((IntStep)strengthStep.GetNextStep())._maxValue = highBound;
                ((IntStep)strengthStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                Console.WriteLine(pointsToUse);
            };

            var staminaStep = new IntStep("Please state a value of you stamina", null, lowBound, highBound);
            int staminaValue = 1;
            staminaStep.OnValidResult += (result) =>
            {
                staminaValue = result;
                pointsToUse -= result;
                highBound = pointsToUse - 3 > 10 ? highBound : pointsToUse - 3;
                ((IntStep)staminaStep.GetNextStep())._maxValue = highBound;
                ((IntStep)staminaStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                Console.WriteLine(pointsToUse);
            };

            var inteligenceStep = new IntStep("Please state a value of you inteligence", null, lowBound, highBound);
            int inteligenceValue = 1;
            inteligenceStep.OnValidResult += (result) =>
            {
                inteligenceValue = result;
                pointsToUse -= result;
                highBound = pointsToUse - 2 > 10 ? highBound : pointsToUse - 2;
                ((IntStep)inteligenceStep.GetNextStep())._maxValue = highBound;
                ((IntStep)inteligenceStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                Console.WriteLine(pointsToUse);
            };

            var luckStep = new IntStep("Please state a value of you luck", null, lowBound, highBound);
            int luckValue = 1;
            luckStep.OnValidResult += (result) =>
            {
                luckValue = result;
                pointsToUse -= result;
                highBound = pointsToUse - 1 > 10 ? highBound : pointsToUse - 1;
                ((IntStep)luckStep.GetNextStep())._maxValue = highBound;
                ((IntStep)luckStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                Console.WriteLine(pointsToUse);
            };

            var agilityStep = new IntStep("Please state a value of you agility", null, lowBound, highBound);
            int agilityValue = 1;
            agilityStep.OnValidResult += (result) =>
            {
                agilityValue = result;
                pointsToUse -= result;
                highBound = pointsToUse > 10 ? highBound : pointsToUse;
                ((IntStep)staminaStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                ((IntStep)agilityStep.GetNextStep())._maxValue = highBound;
            };

            var perceptionStep = new IntStep("Please state a value of you perception", null, lowBound, highBound);
            int perceptionValue = 1;
            perceptionStep.OnValidResult += (result) =>
            {
                perceptionValue = result;
            };

            nameStep.SetNextStep(descriptionStep);
            descriptionStep.SetNextStep(primaryTypeStep);
            primaryTypeStep.SetNextStep(secondaryTypeStep);
            secondaryTypeStep.SetNextStep(introToAttributes);
            introToAttributes.SetNextStep(strengthStep);
            strengthStep.SetNextStep(staminaStep);
            staminaStep.SetNextStep(inteligenceStep);
            inteligenceStep.SetNextStep(luckStep);
            luckStep.SetNextStep(agilityStep);
            agilityStep.SetNextStep(perceptionStep);
            perceptionStep.SetNextStep(null);

            var userChannel = ctx.Channel;
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, nameStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }

            FImon newFimon = new FImon(userID, FImonName, description, primaryType, secondaryType, strengthValue, staminaValue, inteligenceValue, luckValue,
                agilityValue, perceptionValue);

            FimonManager.AddFimon(newFimon);
            await ctx.Channel.SendMessageAsync("FIMON added successfully");
            await ctx.Channel.SendMessageAsync($"{FImonName} {description} {primaryType.ToString()} {secondaryType.ToString()}");
        }

        private static Dictionary<DiscordEmoji, ReactionStepData> GetFImonTypesReactionOptions(CommandContext ctx)
        {
            return new Dictionary<DiscordEmoji, ReactionStepData>
            {
                { DiscordEmoji.FromName(ctx.Client,":fire:"), new ReactionStepData{ Content = "Fire Type", NextStep = null, optionalData = FImonType.Fire }},
                { DiscordEmoji.FromName(ctx.Client,":potable_water:"), new ReactionStepData{ Content = "Water Type", NextStep = null, optionalData = FImonType.Water}},
                { DiscordEmoji.FromName(ctx.Client,":earth_africa:"), new ReactionStepData{ Content = "Ground Type", NextStep = null, optionalData = FImonType.Ground}},
                { DiscordEmoji.FromName(ctx.Client,":cloud_tornado:"), new ReactionStepData{ Content = "Air Type", NextStep = null, optionalData = FImonType.Air}},
                { DiscordEmoji.FromName(ctx.Client,":steam_locomotive:"), new ReactionStepData{ Content = "Steel Type", NextStep = null, optionalData = FImonType.Steel}}
            };
        }

        private static DiscordEmbedBuilder AttributesEmbedInfo()
        {
            var attributesIntroEmbed = new DiscordEmbedBuilder()
            {
                Title = "Attributes Selection Intro",
                Color = DiscordColor.DarkButNotBlack,
                Description = "You get to choose from 6 different attributes where each attribute improves certain properties of your FImon, but decreases others. Maximum amount " +
                            "of point you can insert into an attribute is 10 a minimum is 1"
            };

            attributesIntroEmbed.AddField("Strength", "each point increases your health by 4% and basic attack by 3%, but descreases " +
                "your chance to dodge by 2%");
            attributesIntroEmbed.AddField("Stamina", "each point increases your energy pool by 3%");
            attributesIntroEmbed.AddField("Inteligence", "each point increases experience gained by 4%, but descreases " +
                "your chance to critically hit by 0.5%");
            attributesIntroEmbed.AddField("Luck", "each point increases your chance to critically hit by 1%, but descreases " +
                "your experience gained by 2%");
            attributesIntroEmbed.AddField("Agility", "each point increases your chane to dodge by 2%, but descreases " +
                "your health by 3%");
            attributesIntroEmbed.AddField("Perception", "each point increases your chance to hit by 3%");
            return attributesIntroEmbed;
        }

        [Command("initialiseAbilities")]
        public async Task InitialiseAbilities(CommandContext ctx)
        {
            var myFimon = FimonManager.GetFimon(ctx.User.Id);
            if (myFimon == null)
            {
                await ctx.Channel.SendMessageAsync("Sadly you dont have a FImon");
                return;
            }

            AttackAbility att1 = new AttackAbility(1, AbilityType.AutoAttack, FImonType.Ground,
                "Punch", "Average punch of FI student", 50, 20, 5, 10);
            AttackAbility att2 = new AttackAbility(2, AbilityType.AutoAttack, FImonType.Fire,
                "Kick", "Jan Claud van Damn Kick", 50, 20, 5, 10);
            AttackAbility att3 = new AttackAbility(3, AbilityType.AutoAttack, FImonType.Air,
                "Scratch", "Scratches you like your girlfriend... so not at all", 50, 20, 5, 10);
            //-----------------------
            AttackAbility att4 = new AttackAbility(4, AbilityType.BasicAttack, FImonType.Water,
                "Water gun", "Almighty stream of sodastream water", 50, 20, 5, 10);
            AttackAbility att5 = new AttackAbility(5, AbilityType.BasicAttack, FImonType.Fire,
                "FI Roast", "Average roast you get from a FI student", 50, 20, 5, 10);
            AttackAbility att6 = new AttackAbility(6, AbilityType.BasicAttack, FImonType.Ground,
                "Matematika Drsně a svižně", "Swift attack with a previously mentioned book", 50, 20, 5, 10);
            //-----------------------
            AttackAbility att7 = new AttackAbility(7, AbilityType.SpecialAttack, FImonType.Fire,
                "Odpovednik", "Yet another odpovedník", 50, 20, 5, 10);
            AttackAbility att8 = new AttackAbility(8, AbilityType.SpecialAttack, FImonType.Air,
                "Sleeping powder", "The powder of thats made from tears of PB152 students", 50, 20, 5, 10);
            AttackAbility att9 = new AttackAbility(9, AbilityType.SpecialAttack, FImonType.Steel,
                "Naprosto ez xd", "The greatest line to ever exist", 50, 20, 5, 10);
            //-----------------------
            AttackAbility att10 = new AttackAbility(10, AbilityType.UltimateAttack, FImonType.Fire,
                "Really ni**a?", "The almighty question of FI", 50, 20, 5, 10);
            AttackAbility att11 = new AttackAbility(11, AbilityType.UltimateAttack, FImonType.Air,
                "Kontr strike", "Sadly test neprošel", 50, 20, 5, 10);
            AttackAbility att12 = new AttackAbility(12, AbilityType.UltimateAttack, FImonType.Steel,
                "Bretuna?", "Tunabre...", 50, 20, 5, 10);
            //-----------------------
            DefensiveAbility def1 = new DefensiveAbility(13, AbilityType.DefensiveAbility, FImonType.Steel,
                "Harden", "Gets your... thing... even harder", 50);
            DefensiveAbility def2 = new DefensiveAbility(14, AbilityType.DefensiveAbility, FImonType.Air,
                "Speed", "Impossible to be h for next turn", null, 20);
            DefensiveAbility def3 = new DefensiveAbility(15, AbilityType.DefensiveAbility, FImonType.Air,
                "Basic Heal", "Heal for moderate amount", 50);

            AbilityManager.AddAbility(att1);
            AbilityManager.AddAbility(att2);
            AbilityManager.AddAbility(att3);
            AbilityManager.AddAbility(att4);
            AbilityManager.AddAbility(att5);
            AbilityManager.AddAbility(att6);
            AbilityManager.AddAbility(att7);
            AbilityManager.AddAbility(att8);
            AbilityManager.AddAbility(att9);
            AbilityManager.AddAbility(att10);
            AbilityManager.AddAbility(att11);
            AbilityManager.AddAbility(att12);
            AbilityManager.AddAbility(def1);
            AbilityManager.AddAbility(def2);
            AbilityManager.AddAbility(def3);

            await ctx.Channel.SendMessageAsync("Added new ability");
        }

        [Command("getabilities")]
        public async Task GetAbilities(CommandContext ctx)
        { 
            foreach (var ab in AbilityManager.attackAbilities.Values)
            {
                Console.WriteLine(ab.Name);
                Console.WriteLine(ab.Id);
            }
            foreach (var ab in AbilityManager.defensiveAbilities.Values)
            {
                Console.WriteLine(ab.Name);
                Console.WriteLine(ab.Id);
            }
        }
                
        public async Task SetAttack(CommandContext ctx, AbilityType abilityType)
        {
            var options = new Dictionary<string, TextChoiceData>();
            if (abilityType == AbilityType.DefensiveAbility)
            {
                GenerateDefenseChoiceOption(options, abilityType);
            }
            else
            {
                GenerateAttacksChoiceOptions(options, abilityType);
            }           

            var attackSetStep = new TextChoiceStep($"Select your {abilityType.ToString()}", null, options);
            ulong abilityID = 0;
            attackSetStep.OnValidResult = (result) =>
            {
                abilityID = (ulong) result;
            };

            attackSetStep.SetNextStep(null);


            var userChannel = ctx.Channel;
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, attackSetStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);
            Console.WriteLine(abilityID);

            if (!succeeded) { return; }

            FimonManager.SetAbility(ctx.User.Id, abilityID);
            await ctx.Channel.SendMessageAsync("Ability was set to your FImon successfully");
        }

        private static void GenerateAttacksChoiceOptions(Dictionary<string, TextChoiceData> options, AbilityType abilityType)
        {
            foreach (var ability in AbilityManager.GetAttackAbilities())
            {
                if (ability.AbilityType == abilityType)
                {
                    options.Add(ability.Name, new TextChoiceData(ability.GetDescriptionForMessage(), ability.Id));
                }
            }            
        }

        private static void GenerateDefenseChoiceOption(Dictionary<string, TextChoiceData> options, AbilityType abilityType)
        {
            foreach (var ability in AbilityManager.GetDefensiveAbilities())
            {
                if (ability.AbilityType == abilityType)
                {
                    options.Add(ability.Name, new TextChoiceData(ability.GetDescriptionForMessage(), ability.Id));
                }
            }
        }

        [Command("setautoattack")]
        public async Task SetAutoAttack(CommandContext ctx)
        {
            await SetAttack(ctx, AbilityType.AutoAttack);
        }

        [Command("setbasicattack")]
        public async Task SetBasicAttack(CommandContext ctx)
        {
            await SetAttack(ctx, AbilityType.BasicAttack);
        }

        [Command("setspecialattack")]
        public async Task SetSpecialAttack(CommandContext ctx)
        {
            await SetAttack(ctx, AbilityType.SpecialAttack);
        }

        [Command("setfinalattack")]
        public async Task SetUltimateAttack(CommandContext ctx)
        {
            await SetAttack(ctx, AbilityType.UltimateAttack);
        }

        [Command("setdefensive")]
        public async Task SetDefensiveAbility(CommandContext ctx)
        {
            await SetAttack(ctx, AbilityType.DefensiveAbility);
        }
    }
}
