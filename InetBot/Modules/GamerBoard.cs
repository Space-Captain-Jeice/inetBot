using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InetBot.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static InetBot.Data.User;

namespace InetBot.Modules
{
    public class GamerBoard
    {
        private SocketGuild _guild;

        public GameMatch gameMatch;
        GameMatchFileRoot gameMatchFileRoot;

        public async Task InitMatch(string game, SocketSlashCommand command)
        {
            switch (game)
            {
                case "mk7":

                    var cupList = new List<SelectMenuOptionBuilder>
                    {
                        new SelectMenuOptionBuilder().WithLabel("Mushroom Cup").WithValue("mushroom"),
                        new SelectMenuOptionBuilder().WithLabel("Flower Cup").WithValue("flower"),
                        new SelectMenuOptionBuilder().WithLabel("Star Cup").WithValue("star"),
                        new SelectMenuOptionBuilder().WithLabel("Special Cup").WithValue("special"),
                        new SelectMenuOptionBuilder().WithLabel("Shell Cup").WithValue("shell"),
                        new SelectMenuOptionBuilder().WithLabel("Banana Cup").WithValue("banana"),
                        new SelectMenuOptionBuilder().WithLabel("Leaf Cup").WithValue("leaf"),
                        new SelectMenuOptionBuilder().WithLabel("Lightning Cup").WithValue("lightning"),
                    };

                    var userMenuBuilder = new SelectMenuBuilder()
                        .WithType(ComponentType.UserSelect)
                        .WithCustomId("match-mk7-start-modal-user")
                        .WithMinValues(1)
                        .WithMaxValues(8);

                    var cupMenuBuilder = new SelectMenuBuilder()
                        .WithType(ComponentType.SelectMenu)
                        .WithCustomId("match-mk7-start-modal-cup")
                        .WithOptions(cupList)
                        .WithMinValues(1)
                        .WithMaxValues(1);

                    var mk7Modal = new ModalBuilder()
                        .WithTitle("Start MK7 Match")
                        .WithCustomId("match-mk7-start-modal")
                        //.AddSelectMenu("Cup Select", cupMenuBuilder)
                        .AddSelectMenu("User Select", userMenuBuilder);
                        

                    await command.RespondWithModalAsync(mk7Modal.Build());

                    break;
                default:
                    break;
            }
        }

        public async Task StartMatch(SocketModal modal, SocketGuild guild)
        {
            _guild = guild;
            string game = modal.Data.CustomId.Split("-")[1];

            List<SocketMessageComponentData> components = modal.Data.Components.ToList();

            List<String> users = (List<string>)modal.Data.Components.First().Values.ToList();

            string usersString = "";
            List<ulong> playerIDs = new();

            foreach (var item in users)
            {
                usersString += "__" + guild.GetUser(ulong.Parse(item)).Username + "__\n";
                playerIDs.Add(ulong.Parse(item));
            }

            switch (game)
            {
                case "mk7":
                    gameMatchFileRoot = GameMatchFileRoot.GetMatches();
                    gameMatchFileRoot.GameMatchIndex++;

                    List<MatchPlayer> players = new();

                    foreach (var item in playerIDs)
                    {
                        MatchPlayer mK7Player = new(item, 0);
                        players.Add(mK7Player);
                    }

                    gameMatch = new(gameMatchFileRoot.GameMatchIndex, GameMatch.Game.MK7, players);

                    EmbedBuilder initBuilder = new EmbedBuilder()
                        .WithAuthor($"{modal.User.Username}", modal.User.GetAvatarUrl() ?? modal.User.GetDefaultAvatarUrl())
                        .WithTitle($"__Starting MK7 match **#{gameMatch.id}**!__")
                        .WithDescription($"And ITS LIGHTS OUT AND AWAY WE GO!\n\n" +
                        $"Playing with:\n" +
                        $"{usersString}")
                        .WithFooter("Good luck and have fun!")
                        .WithColor(Color.Green);

                    await modal.FollowupAsync(embed: initBuilder.Build());

                    break;
                default:
                    break;
            }
        }

        public async Task StopMatch(string game, SocketSlashCommand command)
        {
            switch (game)
            {
                case "mk7":
                    List<ButtonBuilder> buttons = new();

                    string replyString = "";

                    foreach (var item in gameMatch.players)
                    {
                        string username = _guild.GetUser(item.userId).Username;

                        replyString += $"__{username}__: {item.score}\n";
                        ButtonBuilder buttonBuilder = new ButtonBuilder($"{username}", $"match-mk7-stop-button-{item.userId}");
                        buttons.Add(buttonBuilder);
                    }

                    ButtonBuilder finishButtonBuilder = new ButtonBuilder($"Finish", $"match-mk7-stop-button-finish", ButtonStyle.Success);
                    buttons.Add(finishButtonBuilder);

                    ActionRowBuilder rowBuilder = new ActionRowBuilder()
                        .AddComponents(buttons.ToArray());

                    var builder = new ComponentBuilderV2()
                        .WithActionRow(rowBuilder);

                    EmbedBuilder stoppingBuilder = new EmbedBuilder()
                        .WithAuthor($"{command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                        .WithTitle($"__Stopping MK7 match **#{gameMatch.id}**!__")
                        .WithDescription($"And they're across the line!\n\n" +
                        $"Please input each players score by pressing the buttons below:\n" +
                        $"{replyString}")
                        .WithFooter("Please answer truthfully.")
                        .WithColor(Color.Orange);

                    await command.RespondAsync(embed: stoppingBuilder.Build(), components: builder.Build());

                    break;
                default:
                    break;
            }

        }

        public async Task SetScoresModal(SocketMessageComponent component, SocketGuild guild)
        {
            string[] customId = component.Data.CustomId.Split('-');

            var setScoreModal = new ModalBuilder()
                .WithTitle($"Set score for {guild.GetUser(ulong.Parse(customId[4]))}")
                .WithCustomId($"match-setscores-modal-{customId[4]}")
                .AddTextInput("Score", $"match-setscores-modal-{customId[4]}");


            await component.RespondWithModalAsync(setScoreModal.Build());
        }

        public async Task SetScores(SocketModal modal, SocketGuild guild)
        {
            string[] customId = modal.Data.CustomId.Split("-");
            int score = int.Parse(modal.Data.Components.First().Value);

            if (score > 40) score = 40;

            string replyString = $"";

            foreach (var item in gameMatch.players)
            {
                if (item.userId == ulong.Parse(customId[3]))
                {
                    item.score = score;
                }

                string username = guild.GetUser(item.userId).Username;
                replyString += $"{username}: {item.score}\n";
            }

            EmbedBuilder stoppingBuilder = new EmbedBuilder()
                .WithAuthor($"{modal.User.Username}", modal.User.GetAvatarUrl() ?? modal.User.GetDefaultAvatarUrl())
                .WithTitle($"__Stopping MK7 match **#{gameMatch.id}**!__")
                .WithDescription($"And they're across the line!\n\n" +
                $"Please input each players score by pressing the buttons below:\n" +
                $"{replyString}")
                .WithFooter("Please answer truthfully.")
                .WithColor(Color.Orange);

            await modal.ModifyOriginalResponseAsync( x =>
            {
                x.Embed = stoppingBuilder.Build();
            });
        }

        public async Task FinishMatch(SocketMessageComponent component, SocketGuild guild)
        {
            string replyString = "";

            List<MatchPlayer> sortedPlayers = gameMatch.players.OrderByDescending(x=> x.score).ToList();

            int x = 1;
            foreach (MatchPlayer player in sortedPlayers)
            {
                string username = guild.GetUser(player.userId).Username;
                replyString += $"**#{x}** {username}: {player.score}\n";
                x++;
            }

            EmbedBuilder stoppingBuilder = new EmbedBuilder()
                .WithAuthor($"{component.User.Username}", component.User.GetAvatarUrl() ?? component.User.GetDefaultAvatarUrl())
                .WithTitle($"__Stopped MK7 match **#{gameMatch.id}**!__")
                .WithDescription($"Let's see how they did!\n" +
                $"{replyString}")
                .WithFooter("Congratulations!")
                .WithColor(Color.Green);

            await component.UpdateAsync(x =>
            {
                x.Embed = stoppingBuilder.Build();
                x.Components = null;
            });

            gameMatchFileRoot.GameMatchList.Add(gameMatch);
            await SaveMatches(gameMatchFileRoot);
            gameMatch = null;
        }

        public async Task GetLeaderboards(SocketSlashCommand command, SocketGuild guild)
        {
            gameMatchFileRoot = GameMatchFileRoot.GetMatches();

            List<GameMatch> gameMatchList = new List<GameMatch>();

            foreach (GameMatch match in gameMatchFileRoot.GameMatchList)
            {
                if (match.type == GameMatch.Game.MK7)
                {
                    gameMatchList.Add(match);
                }
            }

            List<MatchPlayer> players = new();

            foreach (GameMatch match in gameMatchList)
            {
                foreach (MatchPlayer player in match.players)
                {
                    MatchPlayer playerSummed = new(player.userId, player.score);
                    players.Add(playerSummed);
                }
            }

            var peanits = players.GroupBy(s => s.userId).ToDictionary(g => g.Key, g => g.Sum(p => p.score));
            List<MatchPlayer> finalPlayers = new();

            foreach (var item in peanits)
            {
                MatchPlayer player = new(item.Key, item.Value);
                finalPlayers.Add(player);
            }

            string replyString = "";

            finalPlayers = finalPlayers.OrderByDescending(x => x.score).ToList();

            try
            {
                int requestingUserIndex = finalPlayers.FindIndex(x => x.userId.Equals(command.User.Id));
                replyString += $"You are in position **#{requestingUserIndex + 1}** with **{finalPlayers.ElementAt(requestingUserIndex).score}** points!\n\n";
            }
            catch (ArgumentOutOfRangeException e)
            {
                replyString += $"You have not played a match yet!\n\n";
            }

            int x = 1;
            foreach (MatchPlayer player in finalPlayers)
            {
                string username = guild.GetUser(player.userId).Username;
                replyString += $"**#{x}** {username}: {player.score}\n";
                x++;
                if (x == 6) break;
            }

            EmbedBuilder stoppingBuilder = new EmbedBuilder()
                .WithAuthor($"{command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                .WithTitle($"__Here are the top 5 players!__")
                .WithDescription($"{replyString}")
                .WithFooter("Keep playing to reach the top!")
                .WithColor(Color.Green);

            await command.RespondAsync(embed:stoppingBuilder.Build());
        }

        private async Task SaveMatches(GameMatchFileRoot games)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(string.Concat(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "\\gamematches.json"), JsonConvert.SerializeObject(games, Formatting.Indented));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                File.WriteAllText("/home/vendell/inet/gamematches.json", JsonConvert.SerializeObject(games, Formatting.Indented));
            }
        }


    }
}
