using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot_Tutorial.Commands
{
    public class TeamCommands : BaseCommandModule
    {
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
