using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FImonBot.CommandAttributes;
using FImonBot.Game;
using FImonBot.Game.QuizQuestions;
using FImonBot.Handlers.Dialogue;
using FImonBot.Handlers.Dialogue.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FImonBot.Commands
{
    internal class QuizCommands : SharedBaseForCommands
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

            var inputDialogueHandler = new DialogueHandler(ctx.Client, ctx.Channel, ctx.User, optionsStep, true, false);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            if (answer == question.CorrectOption)
            {
                trainer.AddExperience(question.ExpReward);
                await SendCorrectMessage($"Correct answer your received {question.ExpReward}exp", ctx.Channel);
            }
            else
            {
                await SendCorrectMessage($"Incorrect answer", ctx.Channel);
            }

            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        private Dictionary<DiscordEmoji, ReactionStepData> GenerateQuestionOptions(CommandContext ctx, Question question)
        {
            var options = new Dictionary<DiscordEmoji, ReactionStepData>();
            var discordClient = ctx.Client;
            var questionOptions = question.Options;

            Random rnd = new Random();
            string[] shuffledOptions = questionOptions.OrderBy(x => rnd.Next()).ToArray();

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            for (int i = 0; i < question.Options.Count(); i++)
            {
                options.Add(DiscordEmoji.FromName(discordClient, $":{unitsMap[i]}:"), new ReactionStepData { Content = $"{shuffledOptions[i]}", NextStep = null });
            }
            return options;
        }

        [Command("initQuestions")]
        [RequireAdmin]
        public async Task InitQuestions(CommandContext ctx)
        {
            QuizManager.AddQuestion(1, "How speed can Speed be?", 1, "yes", "not much", "a little", "prokes");
            QuizManager.AddQuestion(2, "What is the name of the lord of Outland?", 1, "Ilidan", "Maiev", "Speed", "Thason");
            QuizManager.AddQuestion(3, "What is the name of the lord of Northrend?", 1, "Arthas", "Ilidan", "Muradin", "Stylez");
            QuizManager.AddQuestion(4, "What is the name of the lord of Haskell?", 1, "Pismenka", "Speed", "Vojcek", "Barny", "Dipcak");
            QuizManager.AddQuestion(5, "Which is the best programming language?", 1, "C#", "C#", "C#", "C#", "C#");
            QuizManager.AddQuestion(6, "Which is the best operating system?", 1, "GLORIOUS ALMIGHTY WINDOWS", "Debian", "Mac", "Ubuntu");
        }
    }
}
