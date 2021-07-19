using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using SilverBotDS.Attributes;
using SilverBotDS.Exceptions;
using SilverBotDS.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SilverBotDS.Commands
{
    [Category("hunger games")]
    public class Hunger_Games : BaseCommandModule
    {
        HungerGamesArea[] Areas = new HungerGamesArea[] { new("Farm", "FarmArea.png", "farmarea"), new("Grass Field", "GrassArea.png", "grassarea"), new("Lake area", "LakeArea.png", "lakearea") };
        const int mapsizex = 15;
        const int mapsizey = 15;
        [Command("startgame")]
        [Description("e")]
        [RequireOwner]
        public async Task Start(CommandContext ctx, bool shuffle = false, params DiscordMember[] a)
        {
            await ctx.RespondAsync($"starting game with {a.Length} people in it, generating map, this might take a long time (1-30s)");
            using RandomGenerator rg = new();
            using Image<Rgb24> img = new(256* mapsizex, 256 * mapsizey);
            HungerGamesArea[,] vs =new HungerGamesArea[mapsizex, mapsizey];
            for(int x=0;x< mapsizex; x++)
            {
                for(int y=0;y< mapsizey; y++)
                {
                    vs[x, y] = Areas[rg.Next(0, Areas.Length)];
                    img.Mutate(imgctx => imgctx.DrawImage(Image.Load(vs[x, y].GetAssetStream()),new Point(x*256,y*256),1));
                }
            }
            MemoryStream strm = new();
            img.SaveAsJpeg(strm);
            strm.Position = 0;
            var mapmsg=await ctx.RespondAsync(new DiscordMessageBuilder().WithFile("map.jpeg", strm, true).WithContent("this is the map, you will recive it once again in your dms and you will recive a version showing where you are soon™️"));
            List<HungerGamePlayer> hungerGamePlayers = a.Select(x => new HungerGamePlayer(x)).ToList();
            List<HungerGamePlayer> topThree = new();
            if (shuffle)
            {
                hungerGamePlayers = hungerGamePlayers.OrderBy(_ => Guid.NewGuid()).ToList();
            }
            for (int i = 0; i < hungerGamePlayers.Count; i++)
            {
                hungerGamePlayers[i].Channel = await hungerGamePlayers[i].Member.CreateDmChannelAsync();
                hungerGamePlayers[i].Location = new(rg.Next(0, mapsizex), rg.Next(0, mapsizey));
                string locdesc= vs[hungerGamePlayers[i].Location.X, hungerGamePlayers[i].Location.Y].Codename;
                switch (locdesc)
                {
                    case "farmarea":
                        locdesc = "in a farm";
                        break;
                    case "grassarea":
                        locdesc = "in a grass field";
                        break;
                    case "lakearea":
                        locdesc = "near a lake";
                        break;
                }
                await hungerGamePlayers[i].Channel.SendMessageAsync($"You suddenly wake up and find yourself {locdesc}.");
            }
             while (hungerGamePlayers.Count > 0)
             {
                 for (int i = 0; i < hungerGamePlayers.Count; i++)
                 {
                     await hungerGamePlayers[i].RunTurnAsync(ctx,vs);
                     await hungerGamePlayers[i].AfterOwnTurnTaskAsync();
                 }
                 hungerGamePlayers.RemoveAll(x => x.Health <= 0);
             }
        }
    }
    public class HungerGamesArea
    {
        private const string AssemblyPath = "SilverBotDS.Commands.HungerGamesRecources.Area.";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name for the area</param>
        /// <param name="asset">image name of the area with file extension</param>
        /// <param name="codename">the name of the area in code</param>
        public HungerGamesArea(string name,string asset,string codename)
        {
            Name = name;
            Asset = asset;
            Codename = codename;
        }
        public string Name;
        public string Asset;
        public string Codename;
        public Stream GetAssetStream()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(AssemblyPath + Asset) ?? throw new TemplateReturningNullException(AssemblyPath + Asset);
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
        [Range(0, 100,
        ErrorMessage = "Health for {0} must be between {1} and {2}.")]
        public int Health = 95;
        /// <summary>
        /// The thirst of the player, with 0 meaning getting damage from thirst and 100 being full and 150 being way too full
        /// </summary>
        [Range(0, 150,
        ErrorMessage = "Water for {0} must be between {1} and {2}.")]
        public int Water = 95;
        /// <summary>
        /// Stamina of the player, 0 dying of low stamina, 100 max<br/>
        /// Walking to another plot should take 20<br/>
        /// Searching around a plot should take 12<br/>
        /// </summary>
        [Range(0, 100,
        ErrorMessage = "Stamina for {0} must be between {1} and {2}.")]
        public int Stamina = 80;
        public Point Location { get; set; }
       
        public async Task RunTurnAsync(CommandContext ctx, HungerGamesArea[,] vs)
        {
           var msg= await Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("please make your difficult choice here, you have 10s to respond or i will not let you").AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "sleep", "act like silver aka dont do anything"), new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "move", "move around, NOT IMPLEMENTED"), new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "look", "Look around"), new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "exit", "Leave the game")));
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
                else if(interactivity.Result.Id == "look")
                {
                    string locdesc = vs[Location.X, Location.Y].Codename;
                    switch (locdesc)
                    {
                        case "farmarea":
                            locdesc = "in a farm";
                            break;
                        case "grassarea":
                            locdesc = "in a grass field";
                            break;
                        case "lakearea":
                            locdesc = "near a lake";
                            break;
                    }
                    await Channel.SendMessageAsync($"You look around and see that you are {locdesc}.");
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
