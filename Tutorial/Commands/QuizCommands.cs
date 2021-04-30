using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FImonBot.CommandAttributes;
using FImonBot.Game;
using FImonBot.Handlers.Dialogue;
using FImonBot.Handlers.Dialogue.Steps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FImonBot.Commands
{
    class QuizCommands : SharedBaseForCommands
    {
        [Command("answerQuiz")]
        [RequireNotBanned]
        [RequireNotInAction]
        [RequireChannelNameIncludes("quiz")]
        public async Task AnswerQuiz(CommandContext ctx)
        {
            var trainer = TrainerManager.GetTrainer(authorID);
            if (trainer == null)
            {
                await SendErrorMessage("You dont have a trainer", ctx.Channel);
                return;
            }

            ActionsManager.SetUserInAction(ctx.Member.Id);

            var question = QuizManager.GetRandomQuestion();
            var questionOptions = GenerateQuestionOptions(ctx, question);

            var optionsStep = new ReactionStep(question.QuestionValue, questionOptions);
            string answer = null;
            optionsStep.OnValidResult += result =>
            {
                answer = questionOptions[result].Content;
            };

            var inputDialogueHandler = new DialogueHandler(ctx.Client, ctx.Channel, ctx.User, optionsStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            if (answer == question.CorrectOption)
            {
                trainer.AddExperience(question.ExpReward);
                await SendCorrectMessage($"Correct answer your received {question.ExpReward}exp",ctx.Channel);                
            }
            else
            {
                await SendCorrectMessage($"Incorrect answer", ctx.Channel);
            }     

            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        private Dictionary<DiscordEmoji, ReactionStepData> GenerateQuestionOptions(CommandContext ctx, Game.QuizQuestions.Question question)
        {
            var options = new Dictionary<DiscordEmoji, ReactionStepData>();
            var discordClient = ctx.Client;
            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            for (int i = 0; i < question.Options.Count(); i++)
            {
                options.Add(DiscordEmoji.FromName(discordClient, $":{unitsMap[i]}:"), new ReactionStepData { Content = $"{question.Options[i]}", NextStep = null });
            }
            return options;
        }
    }
}
