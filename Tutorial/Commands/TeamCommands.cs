using Discord_Bot_Tutorial.Handlers.Dialogue;
using Discord_Bot_Tutorial.Handlers.Dialogue.Steps;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot_Tutorial.Commands
{
    public class TeamCommands : BaseCommandModule
    {
        [Command("add")]
        [Description("Adds two numbers together")]
        public async Task Add(CommandContext ctx, [Description("num1")] int numberOne, [Description("num2")] int numberTwo)
        {
            await ctx.Channel.SendMessageAsync((numberOne + numberTwo).ToString()).ConfigureAwait(false);
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

            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, emojiStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }
        }

        [Command("join")] // The actual string they have to type to trigger it        
        public async Task Join(CommandContext ctx)
        {
            var joinEmbed = new DiscordEmbedBuilder()
            {
                Title = "Would you like to join?",
                ImageUrl = ctx.Client.CurrentUser.AvatarUrl,
                Color = DiscordColor.Purple
            };

            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);

            var thumbsUpEmoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var thumbsDownEmoji = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await joinMessage.CreateReactionAsync(thumbsUpEmoji).ConfigureAwait(false);
            await joinMessage.CreateReactionAsync(thumbsDownEmoji).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var reaction = await interactivity.WaitForReactionAsync(x => x.Message == joinMessage && (x.Emoji == thumbsDownEmoji || x.Emoji == thumbsUpEmoji) && x.User == ctx.User);

            if (!reaction.TimedOut)
            {
                var upRole = ctx.Guild.GetRole(815900308801716254);
                var downRole = ctx.Guild.GetRole(815900308801716254);
                if (reaction.Result.Emoji == thumbsUpEmoji)
                {
                    await ctx.Member.GrantRoleAsync(upRole).ConfigureAwait(false);
                    await ctx.Member.RevokeRoleAsync(downRole).ConfigureAwait(false);
                }
                else if (reaction.Result.Emoji == thumbsDownEmoji)
                {
                    await ctx.Member.GrantRoleAsync(downRole).ConfigureAwait(false);
                    await ctx.Member.RevokeRoleAsync(upRole).ConfigureAwait(false);
                }
            }
            await joinMessage.DeleteAsync().ConfigureAwait(false);
        }
    }
}
