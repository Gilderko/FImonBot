using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot_Tutorial.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)] // You can place it only on method or class
    class RequireCategoriesAttribute : CheckBaseAttribute
    {
        public IReadOnlyList<string> CategoryNames { get; }
        public ChannelCheckMode CheckMode { get; }

        public RequireCategoriesAttribute(ChannelCheckMode checkMode, params string[] channelNames)
        {
            CheckMode = checkMode;
            CategoryNames = new ReadOnlyCollection<string>(channelNames);
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
            {
                return Task.FromResult(false);
            }

            bool contains = CategoryNames.Contains(ctx.Channel.Parent.Name, StringComparer.OrdinalIgnoreCase);

            switch (CheckMode)
            {
                case ChannelCheckMode.Any:
                    return Task.FromResult(contains);

                case ChannelCheckMode.None:
                    return Task.FromResult(!contains);

                default:
                    return Task.FromResult(false);
            }
        }
    }
}
