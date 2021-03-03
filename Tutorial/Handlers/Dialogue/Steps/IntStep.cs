using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot_Tutorial.Handlers.Dialogue.Steps
{
    public class IntStep : DialogueStepBase
    {
        private IDialogueStep _nextStep;
        private readonly int? _minValue;
        private readonly int? _maxValue;

        public IntStep(string content,IDialogueStep nextStep,int? minLength = null, int? maxLength = null) : base(content)
        {
            _nextStep = nextStep;
            _minValue = minLength;
            _maxValue = maxLength;
        }

        public Action<int> OnValidResult { get; set; } = delegate { };

        public override IDialogueStep NextStep => _nextStep;

        public void SetNextStep(IDialogueStep nextstep)
        {
            _nextStep = nextstep;
        }

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var embedBuidler = new DiscordEmbedBuilder()
            {
                Title = _content,
                Color = DiscordColor.Blue,
                Description = $"{user.Mention}, please respond down below :)"
            };

            embedBuidler.AddField("To Stop the Dialogue", "User the ?cancel command");

            if (_minValue.HasValue)
            {
                embedBuidler.AddField("Min value: ", $"{_minValue.Value}");
            }
            if (_maxValue.HasValue)
            {
                embedBuidler.AddField("Max value: ", $"{_maxValue.Value}");
            }

            var interactivity = client.GetInteractivity();

            while (true)
            {
                var embded = await channel.SendMessageAsync(embed: embedBuidler).ConfigureAwait(false);

                OnMessageAdded(embded);

                var messageResult = await interactivity.WaitForMessageAsync(x => x.ChannelId == channel.Id && x.Author.Id == user.Id).ConfigureAwait(false);

                if (messageResult.TimedOut)
                {
                    return true;
                }

                OnMessageAdded(messageResult.Result);

                if (messageResult.Result.Content.Equals("?cancel", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!int.TryParse(messageResult.Result.Content,out int inputValue))
                {
                    await TryAgain(channel, $"Your input is not an integer").ConfigureAwait(false);
                    continue;
                }

                if (_minValue.HasValue)
                {
                    if (messageResult.Result.Content.Length < _minValue.Value)
                    {
                        await TryAgain(channel, $"Your input value {inputValue} is too small").ConfigureAwait(false);
                        continue;
                    }
                }
                if (_maxValue.HasValue)
                {
                    if (messageResult.Result.Content.Length > _maxValue.Value)
                    {
                        await TryAgain(channel, $"Your input value {inputValue} is too big").ConfigureAwait(false);
                        continue;
                    }
                }

                OnValidResult(inputValue);

                return false;
            }
        }
    }
}
