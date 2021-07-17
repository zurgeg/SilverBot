using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using SilverBotDS.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SilverBotDS.Commands
{
    [Category("hunger games")]
    public class Hunger_Games : BaseCommandModule
    {
        [Command("startgame")]
        [Description("e")]
        [RequireOwner]
        public async Task Start(CommandContext ctx, bool shuffle = false, params DiscordMember[] a)
        {
            await ctx.RespondAsync($"starting game with {a.Length} people in it");
            List<HungerGamePlayer> hungerGamePlayers = a.Select(x => new HungerGamePlayer(x)).ToList();
            List<HungerGamePlayer> topThree = new();
            if (shuffle)
            {
                hungerGamePlayers = hungerGamePlayers.OrderBy(_ => Guid.NewGuid()).ToList();
            }
            for (int i = 0; i < hungerGamePlayers.Count; i++)
            {
                hungerGamePlayers[i].Channel = await hungerGamePlayers[i].Member.CreateDmChannelAsync();
            }

            while (hungerGamePlayers.Count > 0)
            {
                for (int i = 0; i < hungerGamePlayers.Count; i++)
                {
                    await hungerGamePlayers[i].RunTurnAsync(ctx);
                    await hungerGamePlayers[i].AfterOwnTurnTaskAsync();
                }
                hungerGamePlayers.RemoveAll(x => x.Health <= 0);
            }
        }
    }
    public class HungerGamePlayer
    {
        public HungerGamePlayer(DiscordMember member)
        {
            Member = member;
        }
        /// <summary>
        /// The member the player represents
        /// </summary>
        public DiscordMember Member;
        /// <summary>
        /// The dmchannel, it has to be set before start of game loop
        /// </summary>
        public DiscordDmChannel Channel;
        /// <summary>
        /// The health of the player, should be between 0 and 100
        /// </summary>
        public int Health = 95;
        /// <summary>
        /// The thirst of the player, with 0 meaning getting damage from thirst and 100 being full and 150 being way too full
        /// </summary>
        public int Water = 95;
        /// <summary>
        /// Stamina of the player, 0 dying of low stamina, 100 max<br/>
        /// Walking to another plot should take 20<br/>
        /// Searching around a plot should take 12<br/>
        /// </summary>
        public int Stamina = 80;
        public async Task RunTurnAsync(CommandContext ctx)
        {
           var msg= await Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("please make your difficult choice here, you have 10s to respond or i will cancel you on twitter").AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "sleep", "act like silver aka dont do anything"), new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "exit", "Leave the game")));
           var interactivity=await msg.WaitForButtonAsync(TimeSpan.FromSeconds(10));
            if(interactivity.TimedOut)
            {
                await msg.DeleteAsync();
                await Channel.SendMessageAsync("YOU'RE TOO SLOW");
            }
            else
            {
                if(interactivity.Result.Id == "exit")
                {
                    Health = 0;
                    await ctx.Channel.SendMessageAsync($"{Member.Mention} died of leaving");
                }
                await interactivity.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("great job you clicked on some button"));
            }
        }

        public async Task AfterOwnTurnTaskAsync()
        {
            var channel = await Member.CreateDmChannelAsync();
            if (Health <= 0)
            {
                await channel.SendMessageAsync("You have died.");
            }
            else if (Health < 5)
            {
                await channel.SendMessageAsync("You have less than 5 HP");
            }
            else if (Health < 10)
            {
                await channel.SendMessageAsync("You have less than 10 HP");
            }
            else if (Health < 15)
            {
                await channel.SendMessageAsync("You have less than 15 HP, **regain health** or you will die and lose the game :(");
            }
            else if (Health < 20)
            {
                await channel.SendMessageAsync("You have less than 20 HP, you **really** want to regain health.");
            }
            else if (Health < 30)
            {
                await channel.SendMessageAsync("You have less than 30 HP, you might **really** want to consider regaining health.");
            }
            else if (Health < 50)
            {
                await channel.SendMessageAsync("You have less than 50 HP, you might want to consider regaining health.");
            }
        }
    }
}
