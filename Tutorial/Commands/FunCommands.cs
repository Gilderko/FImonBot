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
using Tutorial.FImons;

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
            if (FImonManager.mapping.ContainsKey(userID))
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
            ElementalTypes primaryType = ElementalTypes.Air;
            primaryTypeStep.OnValidResult += (result) =>
            {
                primaryType = (ElementalTypes)primaryTypeStep.GetOptions()[result].optionalData;
                reactionTypeOptions.Remove(result);
            };

            var secondaryTypeStep = new ReactionStep("Now what is your secondary type?", reactionTypeOptions);
            ElementalTypes secondaryType = ElementalTypes.Air;
            secondaryTypeStep.OnValidResult += (result) =>
            {
                secondaryType = (ElementalTypes)secondaryTypeStep.GetOptions()[result].optionalData;
                reactionTypeOptions.Remove(result);
            };

            DiscordEmbedBuilder attributesIntroEmbed = AttributesEmbedInfo();

            var introToAttributes = new ReactionStep("", new Dictionary<DiscordEmoji, ReactionStepData>
            {
                { DiscordEmoji.FromName(ctx.Client,":thumbsup:"), new ReactionStepData{ Content = "Continue" } }
            });
            introToAttributes.optionalEmbed = attributesIntroEmbed;

            int pointsToUse = 35;
            int lowBound = 1;
            int highBound = 10;

            var strengthStep = new IntStep("Please state a value of you strength", null, lowBound, highBound);
            strengthStep.dynamicOptionalCommentary = pointsToUse.ToString();
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
                highBound = pointsToUse - 4 > 10 ? highBound : pointsToUse - 4;
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
                highBound = pointsToUse - 3 > 10 ? highBound : pointsToUse - 3;
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
                highBound = pointsToUse - 2 > 10 ? highBound : pointsToUse - 2;
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
                highBound = pointsToUse - 1 > 10 ? highBound : pointsToUse - 1;
                ((IntStep)agilityStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                ((IntStep)agilityStep.GetNextStep())._maxValue = highBound;
            };

            var perceptionStep = new IntStep("Please state a value of you perception", null, lowBound, highBound);
            int perceptionValue = 1;
            perceptionStep.OnValidResult += (result) =>
            {
                perceptionValue = result;
                pointsToUse -= result;
                highBound = pointsToUse > 10 ? highBound : pointsToUse;
                ((IntStep)perceptionStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                ((IntStep)perceptionStep.GetNextStep())._maxValue = highBound;
            };

            var abilityPowerStep = new IntStep("Please state a value of you ability power", null, lowBound, highBound);
            int abilityPowerValue = 1;
            perceptionStep.OnValidResult += (result) =>
            {
                abilityPowerValue = result;
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
            perceptionStep.SetNextStep(abilityPowerStep);
            abilityPowerStep.SetNextStep(null);

            var userChannel = ctx.Channel;
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, nameStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }

            FImon newFimon = new FImon(userID, FImonName, description, primaryType, secondaryType, strengthValue, staminaValue, inteligenceValue, luckValue,
                agilityValue, perceptionValue, abilityPowerValue);

            FImonManager.AddFimon(newFimon);
            await ctx.Channel.SendMessageAsync("FIMON added successfully");
            await ctx.Channel.SendMessageAsync($"{FImonName} {description} {primaryType.ToString()} {secondaryType.ToString()}");
        }

        private static Dictionary<DiscordEmoji, ReactionStepData> GetFImonTypesReactionOptions(CommandContext ctx)
        {
            return new Dictionary<DiscordEmoji, ReactionStepData>
            {
                { DiscordEmoji.FromName(ctx.Client,":fire:"), new ReactionStepData{ Content = "Fire Type", NextStep = null, optionalData = ElementalTypes.Fire }},
                { DiscordEmoji.FromName(ctx.Client,":potable_water:"), new ReactionStepData{ Content = "Water Type", NextStep = null, optionalData = ElementalTypes.Water}},
                { DiscordEmoji.FromName(ctx.Client,":earth_africa:"), new ReactionStepData{ Content = "Ground Type", NextStep = null, optionalData = ElementalTypes.Ground}},
                { DiscordEmoji.FromName(ctx.Client,":cloud_tornado:"), new ReactionStepData{ Content = "Air Type", NextStep = null, optionalData = ElementalTypes.Air}},
                { DiscordEmoji.FromName(ctx.Client,":steam_locomotive:"), new ReactionStepData{ Content = "Steel Type", NextStep = null, optionalData = ElementalTypes.Steel}}
            };
        }

        private static DiscordEmbedBuilder AttributesEmbedInfo()
        {
            var attributesIntroEmbed = new DiscordEmbedBuilder()
            {
                Title = "Attributes Selection Intro",
                Color = DiscordColor.DarkButNotBlack,
                Description = "You get to choose from 7 different attributes where each attribute improves certain properties of your FImon, but decreases others. Maximum amount " +
                            "of point you can insert into an attribute is 10 a minimum is 1.\nPOINTS WHICH YOU WILL NOT USE WILL BE DISCARDED!!!"
            };

            attributesIntroEmbed.AddField("Strength", $"each point increases your auto-attack damage by {BaseStats.strengthAutoAttackDamageIncrease}% and increases its cost by {BaseStats.strengthAutoAttackCostIncrease}%");
            attributesIntroEmbed.AddField("Stamina", $"each point increases your energy pool by {BaseStats.staminaEnergyIncrease}% and your health pool by {BaseStats.staminaHealthIncrease}");
            attributesIntroEmbed.AddField("Inteligence", $"each point increases experience gained by {BaseStats.inteligenceExpGainIncrease}%, but decreases your chance to critically hit by {BaseStats.inteligenceCritChanceDecrease}%");
            attributesIntroEmbed.AddField("Luck", $"each point increases your chance to critically hit by {BaseStats.luckCritChanceIncrease}%, but decreases your experience gained by {BaseStats.luckExpGainDecrease}%");
            attributesIntroEmbed.AddField("Agility", $"each point increases your chane to dodge by {BaseStats.agilityDodgeChanceIncrease}%, but decreases your health by {BaseStats.agilityHealthDecrease}%");
            attributesIntroEmbed.AddField("Perception", $"each point increases your chance to hit by {BaseStats.perceptionHitChanceIncrease}%, but decreases your chance to dodge by {BaseStats.perceptionDodgeChanceDecrease}%");
            attributesIntroEmbed.AddField("Ability power", $"each point increases the damage, healing of all your non AUTO-ATTACK abilities by {BaseStats.abilityPowerIntensityIncrease} and increases their cost by {BaseStats.abilityPowerCostIncrease}%");
            return attributesIntroEmbed;
        }

        [Command("initialiseAbilities")]
        public async Task InitialiseAbilities(CommandContext ctx)
        {    
            AttackAbility att1 = new AttackAbility(1, AbilityType.AutoAttack, ElementalTypes.Ground,
                "Punch", "Average punch of FI student",20 , 20, 10, 65, 15);
            AttackAbility att2 = new AttackAbility(2, AbilityType.AutoAttack, ElementalTypes.Fire,
                "Kick", "Jan Claud van Damn Kick", 15, 20, 5, 60, 25);
            AttackAbility att3 = new AttackAbility(3, AbilityType.AutoAttack, ElementalTypes.Air,
                "Scratch", "Scratches you like your girlfriend... so not at all", 10, 15, 5, 75, 0);
            //-----------------------
            AttackAbility att4 = new AttackAbility(4, AbilityType.BasicAttack, ElementalTypes.Water,
                "Water gun", "Almighty stream of sodastream water", 15, 20, 16, 70, 10);
            AttackAbility att5 = new AttackAbility(5, AbilityType.BasicAttack, ElementalTypes.Fire,
                "FI Roast", "Average roast you get from a FI student", 20, 15, 5, 75, 70);
            AttackAbility att6 = new AttackAbility(6, AbilityType.BasicAttack, ElementalTypes.Ground,
                "Matematika Drsně a svižně", "Swift attack with a previously mentioned book", 18, 18, 10, 80, 45);
            //-----------------------
            AttackAbility att7 = new AttackAbility(7, AbilityType.SpecialAttack, ElementalTypes.Fire,
                "Odpovednik", "Yet another odpovedník", 35, 35, 20, 50, 25);
            AttackAbility att8 = new AttackAbility(8, AbilityType.SpecialAttack, ElementalTypes.Air,
                "Sleeping powder", "The powder of thats made from tears of PB152 students", 40, 30, 15, 55, 50);
            AttackAbility att9 = new AttackAbility(9, AbilityType.SpecialAttack, ElementalTypes.Steel,
                "Naprosto ez xd", "The greatest line to ever exist", 30, 40, 10, 50, 35);
            //-----------------------
            AttackAbility att10 = new AttackAbility(10, AbilityType.UltimateAttack, ElementalTypes.Fire,
                "Really ni**a?", "The almighty question of FI", 75, 55, 40, 65, 10);
            AttackAbility att11 = new AttackAbility(11, AbilityType.UltimateAttack, ElementalTypes.Air,
                "Kontr strike", "Sadly test neprošel", 80, 35, 30, 60, 90);
            AttackAbility att12 = new AttackAbility(12, AbilityType.UltimateAttack, ElementalTypes.Steel,
                "Bretuna?", "Tunabre...", 65, 70, 35, 55, 0);
            //-----------------------
            DefensiveAbility def1 = new DefensiveAbility(13, AbilityType.DefensiveAbility, ElementalTypes.Steel,
                "Harden", "Gets your... thing... even harder", 55, 1, 75);            
            DefensiveAbility def3 = new DefensiveAbility(14, AbilityType.DefensiveAbility, ElementalTypes.Air,
                "Basic Heal", "Heal for moderate amount", 25, 2, 25);

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

            FImonManager.SetAbility(ctx.User.Id, abilityID);
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

        [Command("fight")]
        public async Task Fight(CommandContext ctx)
        {
            var mentioned = ctx.Message.MentionedUsers;
            if (mentioned.Count > 1 || mentioned.Count == 0)
            {
                await ctx.Channel.SendMessageAsync("You have to mention someone in order to fight his FImon");
                return;
            }

            var challenger = ctx.User;
            var myFImon = FImonManager.GetFimon(challenger.Id);
            if (myFImon == null)
            {
                await ctx.Channel.SendMessageAsync("You dont have a FImon mate...");
                return;
            }

            var enemyUser = mentioned[0];
            var enemyFImon = FImonManager.GetFimon(enemyUser.Id);
            if (enemyFImon == null)
            {
                await ctx.Channel.SendMessageAsync("The user you have mentioned does not have a FImon");
                return;
            }

            if (myFImon.AutoAttackID == null || myFImon.BasicAttackID == null || myFImon.SpecialAttackID == null ||
                myFImon.FinalAttackID == null || myFImon.DefensiveAbilityID == null)
            {
                await ctx.Channel.SendMessageAsync("Please set up all your abilities before you want to fight");
                return;
            }
            if (enemyFImon.AutoAttackID == null || enemyFImon.BasicAttackID == null || enemyFImon.SpecialAttackID == null ||
                enemyFImon.FinalAttackID == null || enemyFImon.DefensiveAbilityID == null)
            {
                await ctx.Channel.SendMessageAsync("Enemy FImon does not have all of his abilities set up");
                return;
            }

            var random = new Random();
            int indexChoice = random.Next(0,2);
            var options = new List<DiscordUser> { { challenger }, { enemyUser } };
            var first = options[indexChoice];
            var second = options.Find(s => s != first);            

            var firstAttacker = new InCombatFImon(FImonManager.GetFimon(first.Id));
            var secondAttacker = new InCombatFImon(FImonManager.GetFimon(second.Id));

            // display embed who is going first
            var embedShow = new DiscordEmbedBuilder()
            {
                Title = $"The first attacker will be... {firstAttacker.FImonBase.Name}",
                Color = DiscordColor.DarkGreen,
            };

            InCombatFImon currentFightingFImon = firstAttacker;
            InCombatFImon helpFimon = null;
            InCombatFImon waitingFightingFImon = secondAttacker;

            DiscordUser currentFightingUser = first;
            DiscordUser helpUser = null;
            DiscordUser waitingFightingUser = second;

            InCombatFImon winningFImon = null;
            InCombatFImon loosingFImon = null;
            string battleResult = "";
            while (true)
            {
                Dictionary<DiscordEmoji, ReactionStepData> attackOptions = GenerateAttackOptions(ctx, currentFightingFImon);
                
                ulong AttackerAbilityID = 0;
                var AttackStep = new ReactionStep("Select your attack", attackOptions);
                AttackStep.OnValidResult += (result) =>
                {                    
                    AttackerAbilityID = (ulong)AttackStep.GetOptions()[result].optionalData;
                };

                var optionalEmbed = new DiscordEmbedBuilder();
                AttackStep.optionalEmbed = optionalEmbed;                
                AttackStep.optionalEmbed.AddField($"FImon of owner {currentFightingUser.Username}", currentFightingFImon.FImonBase.Name);
                AttackStep.optionalEmbed.AddField($"Dodge chance", currentFightingFImon.GetDodgeChance().ToString());
                AttackStep.optionalEmbed.AddField("Health", $"{currentFightingFImon.health.ToString()}/{currentFightingFImon.maxHealth}");
                AttackStep.optionalEmbed.AddField("Energy", $"{currentFightingFImon.energy.ToString()}/{currentFightingFImon.maxEnergy}");
                AttackStep.optionalEmbed.AddField("Primary type, Secondary type", $"{currentFightingFImon.FImonBase.PrimaryType}, {currentFightingFImon.FImonBase.SecondaryType}"); 

                var userChannel = ctx.Channel;
                var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, currentFightingUser, AttackStep, false, false);

                bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

                Console.WriteLine("finished dialogue");
                if (!succeeded) 
                {
                    Console.WriteLine("Surrender");
                    battleResult = $"{currentFightingFImon.FImonBase.Name} surrendered the fight";
                    winningFImon = waitingFightingFImon;
                    loosingFImon = currentFightingFImon;
                    break; 
                }
                
                string commentary = "";                
                Ability abilityToUse = AbilityManager.GetAbility(AttackerAbilityID);
                int damageToGive = currentFightingFImon.UseAbilityFImon(abilityToUse, waitingFightingFImon, out commentary);

                waitingFightingFImon.health -= damageToGive;

                var report = new DiscordEmbedBuilder()
                {
                    Title = $"Turn of: {currentFightingFImon.FImonBase.Name}",
                    Description = commentary
                };

                await ctx.Channel.SendMessageAsync(embed: report);                
                
                if (waitingFightingFImon.health <= 0)
                {
                    Console.WriteLine("Oponent Died");
                    battleResult = $"{currentFightingFImon.FImonBase.Name} defeated {waitingFightingFImon.FImonBase.Name} in combat";
                    winningFImon = currentFightingFImon;
                    loosingFImon = waitingFightingFImon;
                    break;
                }

                helpFimon = currentFightingFImon;
                currentFightingFImon = waitingFightingFImon;
                waitingFightingFImon = helpFimon;
                helpFimon = null;

                helpUser = currentFightingUser;
                currentFightingUser = waitingFightingUser;
                waitingFightingUser = helpUser;
                helpUser = null;
            }

            int looserExp = random.Next(10,20);
            int winnerExp = random.Next(30,50);

            int winningExpReward = FImonManager.AwardExperience(winningFImon.FImonBase.DiscordUserID, winnerExp);
            int loosingExpReward = FImonManager.AwardExperience(loosingFImon.FImonBase.DiscordUserID, looserExp);
            var winningEmbed = new DiscordEmbedBuilder()
            {
                Title = $"And the winner is: {winningFImon.FImonBase.Name}",
                Color = DiscordColor.Gold,
                Description = $"The winner received {winningExpReward} exp and the looser receives {loosingExpReward} exp\n" + battleResult
            };
            await ctx.Channel.SendMessageAsync(embed: winningEmbed);
        }

        [Command("getfimon")]
        public async Task GetFimon(CommandContext ctx)
        {
            DiscordUser userToFind = ctx.User;

            InCombatFImon currentFightingFImon = new InCombatFImon(FImonManager.GetFimon(userToFind.Id));
            DiscordUser currentFightingUser = userToFind;
            Dictionary<DiscordEmoji, ReactionStepData> attackOptions = GenerateAttackOptions(ctx, currentFightingFImon);
           
            var AttackStep = new ReactionStep("Select your attack", attackOptions);           

            var optionalEmbed = new DiscordEmbedBuilder();
            AttackStep.optionalEmbed = optionalEmbed;
            AttackStep.optionalEmbed.AddField($"FImon of owner {currentFightingUser.Username}", currentFightingFImon.FImonBase.Name);
            AttackStep.optionalEmbed.AddField("Description", currentFightingFImon.FImonBase.Description);
            AttackStep.optionalEmbed.AddField($"Dodge chance", currentFightingFImon.GetDodgeChance().ToString());
            AttackStep.optionalEmbed.AddField("Health", $"{currentFightingFImon.health.ToString()}/{currentFightingFImon.maxHealth}");
            AttackStep.optionalEmbed.AddField("Energy", $"{currentFightingFImon.energy.ToString()}/{currentFightingFImon.maxEnergy}");
            AttackStep.optionalEmbed.AddField("Primary type, Secondary type", $"{currentFightingFImon.FImonBase.PrimaryType}, {currentFightingFImon.FImonBase.SecondaryType}");

            var userChannel = ctx.Channel;
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, userToFind, AttackStep,true,false);

            await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);
        }

        private static Dictionary<DiscordEmoji, ReactionStepData> GenerateAttackOptions(CommandContext ctx, InCombatFImon currentFImon)
        {
            AttackAbility autoAttack = (AttackAbility) AbilityManager.GetAbility((ulong)currentFImon.FImonBase.AutoAttackID);
            AttackAbility basicAttack = (AttackAbility)AbilityManager.GetAbility((ulong)currentFImon.FImonBase.BasicAttackID);
            AttackAbility specialAttack = (AttackAbility)AbilityManager.GetAbility((ulong)currentFImon.FImonBase.SpecialAttackID);
            AttackAbility finalAttack = (AttackAbility)AbilityManager.GetAbility((ulong)currentFImon.FImonBase.FinalAttackID);
            DefensiveAbility defensiveAbility = (DefensiveAbility)AbilityManager.GetAbility((ulong)currentFImon.FImonBase.DefensiveAbilityID);

           
            var options = new Dictionary<DiscordEmoji, ReactionStepData>();            

            if (currentFImon.HaveEnoughEnergyForAbility(autoAttack))
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":crossed_swords:"), new ReactionStepData { Content = $"{autoAttack.Name}\n" + autoAttack.GetDescriptionWithFImon(currentFImon), NextStep = null, optionalData = autoAttack.Id });
            }
            if (currentFImon.HaveEnoughEnergyForAbility(basicAttack))
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":one:"), new ReactionStepData { Content = $"{basicAttack.Name}\n" + basicAttack.GetDescriptionWithFImon(currentFImon), NextStep = null, optionalData = basicAttack.Id });
            }
            if (currentFImon.HaveEnoughEnergyForAbility(specialAttack))
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":two:"), new ReactionStepData { Content = $"{specialAttack.Name}\n" + specialAttack.GetDescriptionWithFImon(currentFImon), NextStep = null, optionalData = specialAttack.Id });
            }
            if (currentFImon.HaveEnoughEnergyForAbility(finalAttack))
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":three:"), new ReactionStepData { Content = $"{finalAttack.Name}\n" + finalAttack.GetDescriptionWithFImon(currentFImon), NextStep = null, optionalData = finalAttack.Id });
            }
            if (currentFImon.HaveEnoughEnergyForAbility(defensiveAbility) && currentFImon.defensiveCharges > 0)
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":shield:"), new ReactionStepData { Content = $"{defensiveAbility.Name}\n" + defensiveAbility.GetDescriptionWithFImon(currentFImon), NextStep = null, optionalData = defensiveAbility.Id });
            }

            ulong def = 0;
            options.Add(DiscordEmoji.FromName(ctx.Client, ":zzz:"), new ReactionStepData { Content = $"Skip turn to generate {BaseStats.energyGainWait}% energy", NextStep = null, optionalData = def });
            return options;
        } 
    }
}
