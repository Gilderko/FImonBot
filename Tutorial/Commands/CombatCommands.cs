using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FImonBot.Game;
using FImonBot.Game.Abilities;
using FImonBot.Game.Combat;
using FImonBot.Game.FImons;
using FImonBot.Handlers.Dialogue;
using FImonBot.Handlers.Dialogue.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FImonBot.CommandAttributes;

namespace FImonBot.Commands
{
    public class CombatCommands : SharedBaseForCommands
    {        
        /// <summary>
        /// Tag a DiscordUser to challenge him to a fight
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("fight")]
        [RequireForFight]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task Fight(CommandContext ctx)
        {
            ChannelAllocator.AllocateRoom(ctx.Channel.Id);
            ActionsManager.SetUserInAction(ctx.Member.Id);

            var mentioned = ctx.Message.MentionedUsers;
            if (mentioned.Count != 1)
            {
                await SendErrorMessage("You need to tag someone to fight him", ctx.Channel);
                ChannelAllocator.FreeRoom(ctx.Channel.Id);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }
            var enemyUser = mentioned[0];
            var challenger = ctx.Member;
            
            if (ActionsManager.IsInAction(enemyUser.Id))
            {
                await SendErrorMessage("Enemy user is already in action", ctx.Channel);
                ChannelAllocator.FreeRoom(ctx.Channel.Id);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            if (TrainerManager.GetTrainer(enemyUser.Id) == null || TrainerManager.GetTrainer(challenger.Id) == null)
            {
                await SendErrorMessage("Please create trainers before the fight", ctx.Channel);
                ChannelAllocator.FreeRoom(ctx.Channel.Id);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            ActionsManager.SetUserInAction(enemyUser.Id);
            var myFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);
            if (myFImon == null)
            {
                await SendErrorMessage($"Fight can not proceed due to {challenger.Username} actions", ctx.Channel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                ActionsManager.RemoveUserFromAction(enemyUser.Id);
                ChannelAllocator.FreeRoom(ctx.Channel.Id);
                return;
            }
            
            var enemyFImon = await SelectYourFImon(enemyUser, ctx.Channel, ctx.Client);
            if (enemyFImon == null)
            {
                await SendErrorMessage($"Fight can not proceed due to {enemyUser.Username} actions", ctx.Channel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                ActionsManager.RemoveUserFromAction(enemyUser.Id);
                ChannelAllocator.FreeRoom(ctx.Channel.Id);
                return;
            }

            if (myFImon.AutoAttackID == null || myFImon.BasicAttackID == null || myFImon.SpecialAttackID == null ||
                myFImon.FinalAttackID == null || myFImon.DefensiveAbilityID == null)
            {
                await SendErrorMessage("Please set up all your abilities before you want to fight", ctx.Channel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                ActionsManager.RemoveUserFromAction(enemyUser.Id);
                ChannelAllocator.FreeRoom(ctx.Channel.Id);
                return;
            }
            if (enemyFImon.AutoAttackID == null || enemyFImon.BasicAttackID == null || enemyFImon.SpecialAttackID == null ||
                enemyFImon.FinalAttackID == null || enemyFImon.DefensiveAbilityID == null)
            {
                await SendErrorMessage("Enemy FImon does not have all of his abilities set up", ctx.Channel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                ActionsManager.RemoveUserFromAction(enemyUser.Id);
                ChannelAllocator.FreeRoom(ctx.Channel.Id);
                return;
            }

            var random = new Random();
            int indexChoice = random.Next(0, 2);
            var options = new List<Tuple<DiscordUser, FImon>> { new Tuple<DiscordUser, FImon>(challenger, myFImon), new Tuple<DiscordUser, FImon>(enemyUser, enemyFImon) };

            var first = options[indexChoice].Item1;
            var second = options[1-indexChoice].Item1;

            var firstFImon = options[indexChoice].Item2;
            var secondFImon = options[1-indexChoice].Item2;

            var firstAttacker = new InCombatFImon(firstFImon);
            var secondAttacker = new InCombatFImon(secondFImon);

            var embedShow = new DiscordEmbedBuilder()
            {
                Title = $"The first attacker will be... {firstAttacker.FImonBase.Name}",
                Color = DiscordColor.DarkGreen,
            };
            await SendCorrectMessage(embedShow, ctx.Channel);

            InCombatFImon currentFightingFImon = firstAttacker;
            InCombatFImon helpFimon = null;
            InCombatFImon waitingFightingFImon = secondAttacker;

            DiscordUser currentFightingUser = first;
            DiscordUser helpUser = null;
            DiscordUser waitingFightingUser = second;

            DiscordUser winningUser = null;
            DiscordUser loosingUser = null;

            InCombatFImon winningFImon = null;
            InCombatFImon loosingFImon = null;
            string battleResult = "";

            Console.WriteLine(first.Username);
            Console.WriteLine(second.Username);
            while (true)
            {
                Dictionary<DiscordEmoji, ReactionStepData> attackOptions = GenerateAttackOptions(ctx, currentFightingFImon);

                ulong AttackerAbilityID = 0;
                var AttackStep = new ReactionStep("Select your attack", attackOptions);
                AttackStep.OnValidResult += (result) =>
                {
                    AttackerAbilityID = (ulong)attackOptions[result].optionalData;
                };

                AttackStep.optionalEmbed = GenerateFImonEmbed(TrainerManager.GetTrainer(currentFightingUser.Id), currentFightingFImon, false);

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
                    winningUser = waitingFightingUser;
                    loosingUser = currentFightingUser;
                    break;
                }

                string commentary = "";
                Ability abilityToUse = AbilityManager.GetAbility(AttackerAbilityID);
                int damageToGive = currentFightingFImon.UseAbilityFImon(abilityToUse, waitingFightingFImon, out commentary);

                waitingFightingFImon.CurrentHealth -= damageToGive;

                var report = new DiscordEmbedBuilder()
                {
                    Title = $"Turn of: {currentFightingFImon.FImonBase.Name}",
                    Description = commentary
                };

                await SendCorrectMessage(report, ctx.Channel);

                if (waitingFightingFImon.CurrentHealth <= 0)
                {
                    Console.WriteLine("Oponent Died");
                    battleResult = $"{currentFightingFImon.FImonBase.Name} defeated {waitingFightingFImon.FImonBase.Name} in combat";
                    winningFImon = currentFightingFImon;
                    loosingFImon = waitingFightingFImon;
                    winningUser = currentFightingUser;
                    loosingUser = waitingFightingUser;
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

            int looserExp = random.Next(10, 20);
            int winnerExp = random.Next(30, 50);

            int winningExpReward = winningFImon.FImonBase.AwardExperience(winnerExp);
            int loosingExpReward = loosingFImon.FImonBase.AwardExperience(looserExp);


            var winningTrainer = TrainerManager.GetTrainer(winningUser.Id);
            winningTrainer.AddExperience(winnerExp / 3);
            winningTrainer.WonBattle();

            var loosingTrainer = TrainerManager.GetTrainer(loosingUser.Id);
            loosingTrainer.AddExperience(winnerExp / 3);
            loosingTrainer.LostBattle();            

            var winningEmbed = new DiscordEmbedBuilder()
            {
                Title = $"And the winner is: {winningFImon.FImonBase.Name}",
                Color = DiscordColor.Gold,
                Description = $"The winner received {winningExpReward} exp and the looser receives {loosingExpReward} exp\n The trainers receive a 3rd of the experience their " +
                $"FImons gained \n" + battleResult
            };
            await SendCorrectMessage(winningEmbed, ctx.Channel);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
            ActionsManager.RemoveUserFromAction(enemyUser.Id);
            ChannelAllocator.FreeRoom(ctx.Channel.Id);
        }
    }
}
