using Discord;
using Discord.Net;
using Discord.WebSocket;
using FuzzySharp;
using FuzzySharp.Extractor;
using FuzzySharp.SimilarityRatio;
using InetBot.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace InetBot.Modules
{
    public class Commands
    {
        bool isSlashCommand;
        bool userHasPerms = false;

        SocketSlashCommand _command;
        SocketMessage _message;

        SocketUser _user;
        SocketUserMessage _userMessage;

        string by = "";
        string valueID = "";

        string[] modCommands = ["ban", "unban", "kick", "unkick", "mute", "warn", "unwarn", "getpunishments", "accept", "deny", "role"];
        string[] commands = ["ban", "unban", "kick", "unkick", "mute", "unmute", "warn", "unwarn", "getpunishments", "deny", "accept", "help", "rule", "rules", "say", "ping", "format", "formst", "formatting", "sd", "sdcard", "piracy", "tnips", "panel", "panels", "ips", "tn", "citra", "emulator", "emulation", "guide", "3ds", "n3ds", "cat", "dog", "otter", "bird", "birb"];
        SocketTextChannel _modChannel;
        
        //3ds:
        public ulong modChannelID = 259878856507392001;
        //tsd:
        //public ulong modChannelID = 440118112977944578;

        //
        // Summary:
        //     Handle a SocketSlashCommand.
        public async Task HandleCommand(SocketSlashCommand command, SocketGuild guild, DiscordSocketClient client)
        {
            _command = command;
            _user = command.User;

            isSlashCommand = true;

            string reason;
            SocketGuildUser guildUser;

            SocketGuildUser guildUser1 = _user as SocketGuildUser;

            _modChannel = guild.GetTextChannel(modChannelID);

            ulong id;

            Console.Write(DateTime.Now.ToString() + " - ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Slash command sent! ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("'" + command.Data.Name + "' ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("'" + command.User.Username + "'\n");
            Console.ResetColor();

            foreach (var item in guildUser1.Roles)
            {
                if (item.Id == 248505026471919618 || item.Id == 259871228406267905) userHasPerms = true;
            }

            switch (command.Data.Name)
            {
                case "ban":
                    guildUser = (SocketGuildUser)command.Data.Options.First().Value;
                    reason = (string)command.Data.Options.ElementAt(1);

                    await HandleBanCommand(guildUser, reason, guild);
                    break;
                case "unban":
                    ulong guildUserId = ulong.Parse(command.Data.Options.First().Value.ToString());

                    await HandleUnbanCommand(guildUserId, guild, client);
                    break;
                case "kick":
                    guildUser = (SocketGuildUser)command.Data.Options.First().Value;
                    reason = (string)command.Data.Options.ElementAt(1);

                    await HandleKickCommand(guildUser, reason, guild);
                    break;
                case "unkick":
                    guildUser = null;
                    by = command.Data.Options.First().Name;
                    if (by == "user") guildUser = (SocketGuildUser)command.Data.Options.First().Options.First().Value;
                    valueID = null;
                    if (by == "id") valueID = (string)command.Data.Options.First().Options.First().Value;

                    await HandleUnkickCommand(guildUser, by, valueID, guild);
                    break;
                case "mute":
                    guildUser = (SocketGuildUser)command.Data.Options.First().Value;
                    string duration = (string)command.Data.Options.ElementAt(1);
                    reason = (string)command.Data.Options.ElementAt(2);

                    await HandleMuteCommand(guildUser, duration, reason, guild);
                    break;
                case "unmute":
                    guildUser = (SocketGuildUser)command.Data.Options.First().Value;

                    await HandleUnmuteCommand(guildUser, guild);
                    break;
                case "warn":
                    guildUser = (SocketGuildUser)command.Data.Options.First().Value;
                    reason = (string)command.Data.Options.ElementAt(1);

                    await HandleWarnCommand(guildUser, reason, guild);
                    break;
                case "unwarn":
                    guildUser = null;
                    by = command.Data.Options.First().Name;
                    if (by == "user") guildUser = (SocketGuildUser)command.Data.Options.First().Options.First().Value;
                    valueID = null;
                    if (by == "id") valueID = (string)command.Data.Options.First().Options.First().Value;

                    await HandleUnwarnCommand(guildUser, by, valueID, guild);
                    break;
                case "getpunishments":
                    guildUser = null;
                    by = command.Data.Options.First().Name;
                    if (by == "target" || by == "moderator") guildUser = (SocketGuildUser)command.Data.Options.First().Options.First().Value;
                    valueID = null;
                    if (by == "id") valueID = (string)command.Data.Options.First().Options.First().Value;

                    await HandleGetpunishmentsCommand(guildUser, by, valueID, guild);
                    break;
                case "role":
                    string action = command.Data.Options.First().Name;
                    guildUser = (SocketGuildUser)command.Data.Options.First().Options.First().Value;
                    IRole role = (IRole)command.Data.Options.First().Options.ElementAt(1).Value;

                    await HandleRoleCommand(action, guildUser, role);
                    break;
                case "deny":
                    id = ulong.Parse(command.Data.Options.First().Value.ToString());
                    await HandleDenyCommand(id, guild);
                    break;
                case "accept":
                    id = ulong.Parse(command.Data.Options.First().Value.ToString());
                    await HandleAcceptCommand(id, guild);
                    break;
                case "help":
                    guildUser = command.User as SocketGuildUser;
                    await HandleHelpCommand(guildUser);
                    break;
            }
        }

        //
        // Summary:
        //     Handle a SocketMessage command.
        public async Task HandleCommand(SocketMessage message, SocketGuild guild, DiscordSocketClient client)
        {
            _message = message;
            _user = message.Author;
            _userMessage = message as SocketUserMessage;

            string msg = message.Content.Remove(0, 1);
            string cmd = msg.Split(" ")[0].ToLower();

            if (cmd == "") return;

            string reason;
            SocketGuildUser guildUser;

            SocketGuildUser guildUser1 = _user as SocketGuildUser;

            _modChannel = guild.GetTextChannel(modChannelID);

            Console.Write(DateTime.Now.ToString() + " - ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("? command sent! ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("'" + cmd + "' ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("'" + _user.Username + "'\n");
            Console.ResetColor();

            ulong id;

            foreach (var item in guildUser1.Roles)
            {
                if (item.Id == 248505026471919618 || item.Id == 259871228406267905) userHasPerms = true;
            }

            EmbedBuilder noPermissionBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle("__No permission!__")
                .WithDescription($"You do not have access to the command `?{cmd}`")
                .WithColor(Color.Red);

            if (char.IsLetterOrDigit(cmd[0]))
            {

                switch (cmd)
                {
                    case "ban":
                        if (!guildUser1.GuildPermissions.BanMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;
                        }

                        if (message.Content.Length <= 5)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?ban <@user> <reason>`\n?ban <@177732626424135680> Said he would never post otters again.")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        if (message.Content.Length == 26)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__No reason provided!__")
                                .WithDescription($":prohibited: Please provide a reason!")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        guildUser = message.MentionedUsers.First() as SocketGuildUser;
                        reason = message.Content.Remove(0, 27);

                        await HandleBanCommand(guildUser, reason, guild);
                        break;
                    case "unban":
                        if (!guildUser1.GuildPermissions.BanMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;

                        }

                        if (message.Content.Length <= 7)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?unban <user id>`\n?unban 177732626424135680")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        ulong guildUserId = ulong.Parse(message.Content.Remove(0, 7));
                        await HandleUnbanCommand(guildUserId, guild, client);
                        break;
                    case "kick":
                        if (!guildUser1.GuildPermissions.KickMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;

                        }

                        if (message.Content.Length <= 6)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?kick <@user> <reason>`\n?kick <@177732626424135680> Didnt post a daily otter picture.")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        if (message.Content.Length == 27)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__No reason provided!__")
                                .WithDescription($":prohibited: Please provide a reason!")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await RespondToTextCommand(errorBuilder);
                            return;
                        }

                        guildUser = message.MentionedUsers.First() as SocketGuildUser;
                        reason = message.Content.Remove(0, 28);

                        await HandleKickCommand(guildUser, reason, guild);
                        break;
                    case "unkick":
                        if (!guildUser1.GuildPermissions.KickMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;

                        }

                        if (message.Content.Length <= 8)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?unkick user/id <@user/punishment id>`\n?unkick <@177732626424135680>\n?unkick 5")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        guildUser = null;
                        by = message.Content.Remove(0, 8).Split(" ")[0];
                        if (by == "user") guildUser = message.MentionedUsers.First() as SocketGuildUser;
                        valueID = null;
                        if (by == "id") valueID = message.Content.Remove(0, 8).Split(" ")[1];

                        await HandleUnkickCommand(guildUser, by, valueID, guild);
                        break;
                    case "mute":
                        if (!guildUser1.GuildPermissions.ModerateMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;
                        }

                        if (message.Content.Length <= 6)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?mute <@user> <duration> <reason>`\n?mute <@177732626424135680> 10m Spamming furry memes.")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        if (message.Content.Length == 27)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__No reason provided!__")
                                .WithDescription($":prohibited: Please provide a reason!")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await RespondToTextCommand(errorBuilder);
                            return;
                        }

                        guildUser = message.MentionedUsers.First() as SocketGuildUser;
                        var duration = message.Content.Remove(0, 28).Split(" ")[0];
                        reason = message.Content.Remove(0, 28 + duration.Length + 1);

                        await HandleMuteCommand(guildUser, duration, reason, guild);
                        break;
                    case "unmute":
                        if (!guildUser1.GuildPermissions.ModerateMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;
                        }

                        if (message.Content.Length <= 8)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?unmute <@user>\n`?unmute <@177732626424135680>")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        guildUser = message.MentionedUsers.First() as SocketGuildUser;

                        await HandleUnmuteCommand(guildUser, guild);
                        break;
                    case "warn":
                        if (!guildUser1.GuildPermissions.KickMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;

                        }

                        if (message.Content.Length <= 5)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?warn <@user> <reason>`\n?warn <@177732626424135680> Sending a risque meme.")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        if (message.Content.Length == 27)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__No reason provided!__")
                                .WithDescription($":prohibited: Please provide a reason!")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await RespondToTextCommand(errorBuilder);
                            return;
                        }

                        guildUser = message.MentionedUsers.First() as SocketGuildUser;
                        reason = message.Content.Remove(0, 28);

                        await HandleWarnCommand(guildUser, reason, guild);
                        break;
                    case "unwarn":
                        if (!guildUser1.GuildPermissions.KickMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;

                        }

                        if (message.Content.Length <= 8)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?unwarn user/id <@user/punishment id>`\n?unwarn <@177732626424135680>\n?unwarn 68")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        guildUser = null;
                        by = message.Content.Remove(0, 8).Split(" ")[0];
                        if (by == "user") guildUser = message.MentionedUsers.First() as SocketGuildUser;
                        valueID = null;
                        if (by == "id") valueID = message.Content.Remove(0, 8).Split(" ")[1];

                        await HandleUnwarnCommand(guildUser, by, valueID, guild);
                        break;
                    case "getpunishments":
                        if (!guildUser1.GuildPermissions.KickMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;

                        }

                        if (message.Content.Length <= 16)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__Syntax:__")
                                .WithDescription($"`?getpunishments mod/target/id <@mod/@target/punishment id>`\n?getpunishments mod <@177732626424135680>\n?getpunishments target <@246050963922616320>\n?getpunishments id 45")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await _userMessage.ReplyAsync(embed: errorBuilder.Build());
                            return;
                        }

                        guildUser = null;
                        by = message.Content.Remove(0, 16).Split(" ")[0];
                        if (by == "mod") by = "moderator";
                        if (by == "target" || by == "moderator") guildUser = message.MentionedUsers.First() as SocketGuildUser;
                        valueID = null;
                        if (by == "id") valueID = message.Content.Remove(0, 16).Split(" ")[1];

                        await HandleGetpunishmentsCommand(guildUser, by, valueID, guild);
                        break;
                    case "deny":
                        if (!userHasPerms)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;
                        }

                        if (message.Content.Length <= 5)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__No member provided!__")
                                .WithDescription($":prohibited: Please provide a member!")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await RespondToTextCommand(errorBuilder);
                            return;
                        }

                        id = ulong.Parse(message.Content.Remove(0, 6));

                        await HandleDenyCommand(id, guild);
                        break;
                    case "accept":
                        if (!userHasPerms)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;
                        }

                        if (message.Content.Length <= 7)
                        {
                            var errorBuilder = new EmbedBuilder()
                                .WithAuthor($"{message.Author.Username} [{message.Author.Id}]", message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                                .WithTitle("__No member provided!__")
                                .WithDescription($":prohibited: Please provide a member!")
                                .WithColor(Color.Red)
                                .WithCurrentTimestamp();

                            await RespondToTextCommand(errorBuilder);
                            return;
                        }

                        id = ulong.Parse(message.Content.Remove(0, 8));

                        await HandleAcceptCommand(id, guild);
                        break;
                    case "help":
                        guildUser = message.Author as SocketGuildUser;
                        await HandleHelpCommand(guildUser);
                        break;
                    case "rule":
                    case "rules":
                        int rule = int.Parse(message.Content.Split(" ")[1]);

                        await HandleRulesCommand(rule);
                        break;
                    case "say":
                        if (!guildUser1.GuildPermissions.KickMembers)
                        {
                            await _userMessage.ReplyAsync(embed: noPermissionBuilder.Build());
                            return;
                        }

                        await HandleSayCommand();
                        break;
                    case "ping":
                        await HandlePingCommand();
                        break;
                    case "format":
                    case "formst":
                    case "formatting":
                        await HandleFormatCommand();
                        break;
                    case "sd":
                    case "sdcard":
                        await HandleSDCommand();
                        break;
                    case "piracy":
                        await HandlePiracyCommand();
                        break;
                    case "tnips":
                    case "panel":
                    case "panels":
                    case "ips":
                    case "tn":
                        await HandleScreenCommand();
                        break;
                    case "citra":
                    case "emulator":
                    case "emulation":
                        await HandleCitraCommand();
                        break;
                    case "guide":
                        string section = "";
                        if (message.Content.Length > 7) section = message.Content.Remove(0, 7);

                        await HandleGuideCommand(section);
                        break;
                    case "3ds":
                    case "n3ds":
                        await HandleDiffCommand();
                        break;
                    case "cat":
                        await HandleCatCommand();
                        break;
                    case "dog":
                        await HandleDogCommand();
                        break;
                    case "otter":
                        await HandleOtterCommand();
                        break;
                    case "bird":
                    case "birb":
                        await HandleBirdCommand();
                        break;
                    case "idiot":
                        await HandleIdiotCommand();
                        break;
                    default:
                        await HandleUnknownCommand(cmd);
                        break;
                }
            }
        }


        private async Task RespondToSlashCommand(EmbedBuilder embedBuilder)
        {
            if (!modCommands.Any(_command.CommandName.Contains))
            {
                await _command.RespondAsync(embed: embedBuilder.Build(), ephemeral: false);
            }
            else
            {
                await _command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
                await _modChannel.SendMessageAsync(embed: embedBuilder.Build());
            }
        }

        private async Task RespondToTextCommand(EmbedBuilder embedBuilder)
        {
            if (!modCommands.Any(_message.Content.Contains))
            {
                await _userMessage.ReplyAsync(embed: embedBuilder.Build());
            }
            else
            {
                await _userMessage.DeleteAsync();
                await _modChannel.SendMessageAsync(embed: embedBuilder.Build());
            }
        }

        private async Task RespondToInfoCommand(EmbedBuilder embedBuilder)
        {
            if (_userMessage.Reference != null)
            {
                await _userMessage.DeleteAsync();
                await _userMessage.ReferencedMessage.ReplyAsync(embed: embedBuilder.Build());
            }
            else
            {
                await _userMessage.ReplyAsync(embed: embedBuilder.Build());
            }
        }

        private async Task HandleHelpCommand(SocketGuildUser guildUser)
        {
            if (guildUser.GuildPermissions.KickMembers)
            {
                var modReplyBuilder = new EmbedBuilder()
                    .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                    .WithTitle("Inet-Kun Moderator Help")
                    .WithDescription("**Inet is your Moderation and Modmail bot for the r/3DS Discord!**\n" +
                    "Here is an overview of the commands with examples! The '?' commands work the same way.\n" +
                    "__**Applying punishments**__\n" +
                    "`/warn <@user> <reason>`\n" +
                    "'/warn <@177732626424135680> Sending a risque meme.'\n" +
                    "Warns the specified user.\n\n" +
                    "`/mute <@user> <duration> <reason>`\n" +
                    "'/mute <@177732626424135680> 10m Spamming furry memes.'\n" +
                    "'/mute <@177732626424135680> 2h He just keeps spamming em.'\n" +
                    "'/mute <@177732626424135680> 7d I have had enough.'\n" +
                    "Times out the specified user for a specified duration. Durations are combineable.\n\n" +
                    "`/kick <@user> <reason>`\n" +
                    "'/kick <@177732626424135680> Didnt post a daily otter picture.'\n" +
                    "Kicks the specified user.\n\n" +
                    "`/ban <@user> <reason>`\n" +
                    "'/ban <@177732626424135680> Said he would never post otters again.'\n" +
                    "Bans the specified user.\n\n" +
                    "You can undo all punishments with `unwarn`, `unmute`, `unkick` and `unban`. unwarn and unkick will just disable the punishments for the user.\n\n" +
                    "__**Looking up punishments**__\n" +
                    "In case you want to look up past punishments, you can do so by the punishment ID, the executing moderator or the target user.\n" +
                    "`/getpunishments <id/mod/target> <id/@user>`\n" +
                    "'/getpunishments id 17'\n" +
                    "'/getpunishments moderator <@177732626424135680>'\n" +
                    "'/getpunishments target <@177732626424135680>'");

                if (isSlashCommand) await RespondToSlashCommand(modReplyBuilder);
                else await RespondToTextCommand(modReplyBuilder);
            }
            
            var userReplyBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle("Inet-Kun User Help")
                .WithDescription("**Inet is your Fun and Modmail bot for the r/3DS Discord!**\n" +
                "Here is an overview of the commands with examples!\n\n" +
                "`?otter/dog/cat/bird`\n" + 
                "Gets a random image of your favourite critter.\n" +
                "`?format/sd/piracy/panel/citra/n3ds`\n" +
                "Provides information about various topics.\n" +
                "`?guide <transfer, cfwupdate, systemupdate, regionchange>`\n" +
                "Gives you information about guides. Optionally points you to guide sections.\n" +
                "`?rule <1-10>`\n" +
                "Shows you the specified rule.\n" +
                "`?ping`\n" +
                "Get the bots ping to discord.");

            if (isSlashCommand) await RespondToSlashCommand(userReplyBuilder);
            else await RespondToTextCommand(userReplyBuilder);
        }

        private async Task HandleRulesCommand(int rule)
        {
            string title = "Oops!";
            string description = "Something went wrong!";
            Color color = Color.DarkerGrey;

            switch (rule)
            {
                //9
                case 1:
                    title = "Rule 1: Be nice";
                    description = "Treat all users in the server with respect and kindness. Everyone is entitled to disagree and have their own opinions, " +
                        "but do so in a civil and clean way. Remember, there is an actual person on the other side of the screen.";
                    color = Color.Green;
                    break;
                case 2:
                    title = "Rule 2: No spamming";
                    description = "No spamming or trolling. This includes, but is not limited to: excessive bot commands, pings, images, and links to other websites. " +
                        "It's completely unnecessary and just clogs and disrupts the chat.";
                    color = Color.Orange;
                    break;
                case 3:
                    title = "Rule 3: No Trading";
                    description = "Trading, begging, or selling of any kind is not allowed. We have no way of keeping track of any kinds of transactions of this nature, " +
                        "nor are we responsible for any missing or lost packages. Take things like this to the appropriate sub on reddit.";
                    color = Color.LightOrange;
                    break;
                case 4:
                    title = "Rule 4: SFW only";
                    description = "NSFW content is not allowed. This should go without saying. We are a PG-13, user-friendly server and subreddit consisting of people of all ages. " +
                        "No one wants to see something inappropriate. Take all of that content far away from here.";
                    color = Color.Red;
                    break;
                case 5:
                    title = "Rule 5: No self-promotion";
                    description = "Self-promotion/advertising, links to other Discord servers, or affiliate links are not permitted. Content in chat should keep users engaged and relevant " +
                        "to the topic at hand, not stray away from it.";
                    color = Color.Magenta;
                    break;
                case 6:
                    title = "Rule 6: No spoilers";
                    description = "No spoilers.Just don't do it. Some people don't like being spoiled or just aren't up to date with the latest news. " +
                        "If there is something you are itching to get out, at least start your message with a spoiler warning or take it to PM.";
                    color = Color.Teal;
                    break;
                case 7:
                    title = "Rule 7: No piracy";
                    description = "While homebrew and flashcart discussion is allowed, talk about piracy or links that redirect to ROM/emulator download sites is strictly prohibited. " +
                        "It's illegal and can lead to all sorts of trouble, simple as that.";
                    color = Color.DarkGrey;
                    break;
                case 8:
                    title = "Rule 8: Stay on topic";
                    description = "Use the appropriate channel. The server is made for ease of use and for everyone to enjoy and their experience on Discord. " +
                        "Use it correctly and to your advantage. It helps keeps the server clean and organized.";
                    color = Color.Purple;
                    break;
                case 9:
                    title = "Rule 9: Obey the mods";
                    description = "Obey mods at all times. If a mod tells you something, it's in your best interest to listen to them. " +
                        "We are always here to help keep the server running and in good shape in conjunction with the subreddit.";
                    color = Color.DarkMagenta;
                    break;
                case 10:
                    title = "Rule 10: Have fun!";
                    description = "Do not break this one.";
                    color = Color.Blue;
                    break;
                case 11:
                    title = "Rule 11: There is no rule 11";
                    description = "Go away.";
                    color = Color.Parse("#ff00ff");
                    break;
                case 34:
                    title = "Rule 34: If it exists, it's not on this server";
                    description = "Aren't you a funny one";
                    color = Color.Parse("#aae5a4");
                    break;
                case 42:
                    title = "Rule 42: The answer";
                    description = "To life, the universe, and everything.";
                    color = Color.Parse("#000000");
                    break;
                case 621:
                    title = "Rule 621: Why did you type this";
                    description = "Rules of furry convention hygene:\n6 hours of sleep per night.\n2 meals per day.\n1 shower per day.\n";
                    color = Color.Parse("#012e56");
                    break;
                default:
                    title = "";
                    break;
            }

            var replyBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(color)
                .WithDescription(description);

            await RespondToInfoCommand(replyBuilder);
        }

        private async Task HandleBanCommand(SocketGuildUser guildUser, string reason, SocketGuild guild)
        {
            if (guildUser.GuildPermissions.KickMembers)
            {
                EmbedBuilder staffMemberPunish = new EmbedBuilder()
                    .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                    .WithTitle("__I can't do that!__")
                    .WithDescription($"You can't ban other staff members.")
                    .WithColor(Color.Red);

                if (isSlashCommand) await RespondToSlashCommand(staffMemberPunish);
                else await RespondToTextCommand(staffMemberPunish);

                return;
            }

            //Create Punishment in DB and save
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            punishments.punishmentIndex++;

            Punishment punishment = new();
            punishment.targetID = guildUser.Id;
            punishment.type = Punishment.Type.BAN;
            punishment.reason = reason;
            punishment.duration = "N/A";
            punishment.modID = _user.Id;
            punishment.punishmentID = punishments.punishmentIndex;
            punishment.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            punishment.active = true;
            punishment.notifMsgID = 0;

            //Create Mod Log
            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle("__Ban applied successfully__")
                .WithDescription($":white_check_mark: `{guildUser.Username}` [{guildUser.Id}] has been banned for __{reason}__. #{punishment.punishmentID}")
                .WithColor(Color.Red)
                .WithCurrentTimestamp();

            //Create User DM
            var warnBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle("**__Ooops...It looks like you have broken the rules of the server.__**")
                .WithDescription($"You have been banned for __{reason}__.")
                .AddField("Punishment ID", $"#{punishment.punishmentID}", true)
                .AddField("Punishent Type", "BAN", true)
                .AddField("Note", "If you disagree with the action taken, please visit [this link](https://forms.gle/CMm8jPAxQCSoGYVY8)", false)
                .WithColor(Color.LightOrange)
                .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                .WithFooter("By joining /r/3DS, you agree that you have read our rules and that you will follow them.\r\nHowever, you have not, and this has led to a punishment.");

            //Send both

            try
            {
                punishment.notifMsgID = guildUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            }
            catch (AggregateException e)
            {
                e.Handle((x) =>
                {
                    if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                    {
                        responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification and won't be able to appeal.");
                        return true;
                    }

                    return false;
                });
            }

            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);

            punishments.punishmentList.Add(punishment);

            await SavePunishment(punishments);

            await guildUser.BanAsync(0, $"{reason} #{punishment.punishmentID}");
        }

        private async Task HandleUnbanCommand(ulong guildUserId, SocketGuild guild, DiscordSocketClient client)
        {
            var user = client.GetUserAsync(guildUserId).Result;

            try
            {
                await guild.RemoveBanAsync(guildUserId);
            }
            catch (HttpException e)
            {
                if (e.DiscordCode == DiscordErrorCode.UnknownBan)
                {
                    var notbannedBuilder = new EmbedBuilder()
                        .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                        .WithTitle("__User not banned!__")
                        .WithDescription($":x: `{user.Username}` [{user.Id}] is not banned or I couldn't find their ban.")
                        .WithColor(Color.Green)
                        .WithCurrentTimestamp();

                    if (isSlashCommand) await RespondToSlashCommand(notbannedBuilder);
                    else await RespondToTextCommand(notbannedBuilder);

                    return;
                }
            }

            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            List<Punishment> reversedPunishments = new();

            foreach (var item in punishments.punishmentList)
            {
                reversedPunishments.Add(item);
            }
            reversedPunishments.Reverse();

            foreach (var reversedItem in reversedPunishments)
            {
                if (reversedItem.targetID == guildUserId && reversedItem.type == Punishment.Type.BAN && reversedItem.active)
                {
                    foreach (var item in punishments.punishmentList)
                    {
                        if (item.punishmentID == reversedItem.punishmentID)
                        {
                            var responseBuilder = new EmbedBuilder()
                                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                                .WithTitle("__Unban applied successfully__")
                                .WithDescription($":white_check_mark: `{user.Username}` [{user.Id}] has been unbanned, their punishment **#{item.punishmentID}** has been set to inactive.")
                                .WithColor(Color.Green)
                                .WithCurrentTimestamp();

                            var notifBuilder = new EmbedBuilder()
                                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                                .WithTitle("__You have been unbanned")
                                .WithDescription($"Your ban **#{item.punishmentID}** with reason `{item.reason}` has been set to inactive.")
                                .WithColor(Color.Green)
                                .WithCurrentTimestamp();

                            try
                            {
                                await user.SendMessageAsync(embed: notifBuilder.Build());
                            }
                            catch (AggregateException e)
                            {
                                e.Handle((x) =>
                                {
                                    if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                                    {
                                        responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification.");
                                        return true;
                                    }

                                    return false;
                                });
                            }


                            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
                            else await RespondToTextCommand(responseBuilder);

                            item.active = false;
                            await SavePunishment(punishments);
                            break;
                        }
                    }
                }
            }
        }

        private async Task HandleKickCommand(SocketGuildUser guildUser, string reason, SocketGuild guild)
        {
            if (guildUser.GuildPermissions.KickMembers)
            {
                EmbedBuilder staffMemberPunish = new EmbedBuilder()
                    .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                    .WithTitle("__I can't do that!__")
                    .WithDescription($"You can't kick other staff members.")
                    .WithColor(Color.Red);

                if (isSlashCommand) await RespondToSlashCommand(staffMemberPunish);
                else await RespondToTextCommand(staffMemberPunish);

                return;
            }

            //Create Punishment in DB
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            punishments.punishmentIndex++;

            Punishment punishment = new();
            punishment.targetID = guildUser.Id;
            punishment.type = Punishment.Type.KICK;
            punishment.reason = reason;
            punishment.duration = "N/A";
            punishment.modID = _user.Id;
            punishment.punishmentID = punishments.punishmentIndex;
            punishment.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            punishment.active = true;

            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle("__Kick applied successfully__")
                .WithDescription($":white_check_mark: `{guildUser.Username}` [{guildUser.Id}] has been kicked for __{reason}__. #{punishment.punishmentID}")
                .WithColor(Color.Orange)
                .WithCurrentTimestamp();

            //Create User DM
            var warnBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle("**__Ooops...It looks like you have broken the rules of the server.__**")
                .WithDescription($"You have been kicked for __{reason}__.")
                .AddField("Punishment ID", $"#{punishment.punishmentID}", true)
                .AddField("Punishent Type", "KICK", true)
                .AddField("Note", "If you disagree with the action taken, please reply to this message to open a ModMail ticket.", false)
                .WithColor(Color.LightOrange)
                .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                .WithFooter("By joining /r/3DS, you agree that you have read our rules and that you will follow them.\r\nHowever, you have not, and this has led to a punishment.");

            //Send both and save notification message ID

            try
            {
                punishment.notifMsgID = guildUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            }
            catch (AggregateException e)
            {
                e.Handle((x) =>
                {
                    if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                    {
                        responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification and won't be able to open a modmail.");
                        return true;
                    }

                    return false;
                });
            }

            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);

            //save punishment in DB
            punishments.punishmentList.Add(punishment);

            await SavePunishment(punishments);

            await guildUser.KickAsync($"{reason} #{punishment.punishmentID}");
        }

        private async Task HandleUnkickCommand(SocketGuildUser? guildUser, string by, string? valueID, SocketGuild guild)
        {
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            List<Punishment> reversedPunishments = new();

            foreach (var item in punishments.punishmentList)
            {
                reversedPunishments.Add(item);
            }
            reversedPunishments.Reverse();

            switch (by)
            {
                case "id":

                    foreach (var item in punishments.punishmentList)
                    {
                        if (item.punishmentID == ulong.Parse(valueID))
                        {
                            var user = guild.GetUser(item.targetID);

                            var responseBuilder = new EmbedBuilder()
                                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                                .WithTitle("__Unkick applied successfully__")
                                .WithDescription($":white_check_mark: `{user.Username}` [{user.Id}] has been unkicked, their punishment **#{item.punishmentID}** has been set to inactive.")
                                .WithColor(Color.Green)
                                .WithCurrentTimestamp();

                            var notifBuilder = new EmbedBuilder()
                                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                                .WithTitle("__You have been unkicked__")
                                .WithDescription($"Your kick **#{item.punishmentID}** with reason `{item.reason}` has been set to inactive.")
                                .WithColor(Color.Green)
                                .WithCurrentTimestamp();

                            try
                            {
                                await user.SendMessageAsync(embed: notifBuilder.Build());
                            }
                            catch (AggregateException e)
                            {
                                e.Handle((x) =>
                                {
                                    if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                                    {
                                        responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification.");
                                        return true;
                                    }

                                    return false;
                                });
                            }

                            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
                            else await RespondToTextCommand(responseBuilder);

                            item.active = false;
                            await SavePunishment(punishments);
                            break;
                        }
                    }
                    break;
                case "user":

                    foreach (var reversedItem in reversedPunishments)
                    {
                        if (reversedItem.targetID == guildUser.Id && reversedItem.type == Punishment.Type.KICK && reversedItem.active)
                        {
                            foreach (var item in punishments.punishmentList)
                            {
                                if (item.punishmentID == reversedItem.punishmentID)
                                {
                                    var responseBuilder = new EmbedBuilder()
                                        .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                                        .WithTitle("__Unkick applied successfully__")
                                        .WithDescription($":white_check_mark: `{guildUser.Username}` [{guildUser.Id}] has been unkicked, their punishment **#{item.punishmentID}** has been set to inactive.")
                                        .WithColor(Color.Green)
                                        .WithCurrentTimestamp();

                                    var notifBuilder = new EmbedBuilder()
                                        .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                                        .WithTitle("__You have been unwarned__")
                                        .WithDescription($"Your kick **#{item.punishmentID}** with reason `{item.reason}` has been set to inactive.")
                                        .WithColor(Color.Green)
                                        .WithCurrentTimestamp();

                                    try
                                    {
                                        await guildUser.SendMessageAsync(embed: notifBuilder.Build());
                                    }
                                    catch (AggregateException e)
                                    {
                                        e.Handle((x) =>
                                        {
                                            if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                                            {
                                                responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification.");
                                                return true;
                                            }

                                            return false;
                                        });
                                    }

                                    if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
                                    else await RespondToTextCommand(responseBuilder);

                                    item.active = false;
                                    await SavePunishment(punishments);
                                    return;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private async Task HandleMuteCommand(SocketGuildUser guildUser, string duration, string reason, SocketGuild guild)
        {
            if (guildUser.GuildPermissions.KickMembers)
            {
                EmbedBuilder staffMemberPunish = new EmbedBuilder()
                    .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                    .WithTitle("__I can't do that!__")
                    .WithDescription($"You can't time out other staff members.")
                    .WithColor(Color.Red);

                if (isSlashCommand) await RespondToSlashCommand(staffMemberPunish);
                else await RespondToTextCommand(staffMemberPunish);

                return;
            }

            //Create Punishment in DB and save
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            punishments.punishmentIndex++;

            Punishment punishment = new();
            punishment.targetID = guildUser.Id;
            punishment.type = Punishment.Type.MUTE;
            punishment.reason = reason;
            punishment.duration = duration;
            punishment.modID = _user.Id;
            punishment.punishmentID = punishments.punishmentIndex;
            punishment.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            punishment.active = true;

            string days = "0";
            string hours = "0";
            string minutes = "0";

            //dont mind this
            if (duration.Contains("d"))
            {
                string[] splitD = duration.Split("d");
                days = splitD[0];
                if (splitD[1].Contains("h"))
                {
                    string[] splitH = splitD[1].Split("h");
                    hours = splitH[0];
                    if (splitH[1].Contains("m"))
                    {
                        string[] splitM = splitH[1].Split("m");
                        minutes = splitM[0];
                    }
                }

            }
            else if (duration.Contains("h"))
            {
                string[] splitH = duration.Split("h");
                hours = splitH[0];
                if (splitH[1].Contains("m"))
                {
                    string[] splitM = splitH[1].Split("m");
                    minutes = splitM[0];
                }
            }
            else if (duration.Contains("m"))
            {
                string[] splitM = duration.Split("m");
                minutes = splitM[0];
            }

            await guildUser.SetTimeOutAsync(new TimeSpan(int.Parse(days), int.Parse(hours), int.Parse(minutes),0));

            //message duration builder
            string messageDuration = "";
            if (days != "0") messageDuration = string.Concat(days, " day(s) ");
            if (hours != "0") messageDuration = string.Concat(messageDuration, hours, " hours ");
            if (minutes != "0") messageDuration = string.Concat(messageDuration, minutes, " minutes");

            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle("__Mute applied successfully__")
                .WithDescription($":white_check_mark: `{guildUser.Username}` [{guildUser.Id}] has been muted for __{reason}__ for __{messageDuration}__. #{punishment.punishmentID}")
                .WithColor(Color.LightOrange)
                .WithCurrentTimestamp();

            //Create User DM
            var warnBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle("**__Ooops...It looks like you have broken the rules of the server.__**")
                .WithDescription($"You have been muted for __{reason}__ for __{messageDuration}__.")
                .AddField("Punishment ID", $"#{punishment.punishmentID}", true)
                .AddField("Punishent Type", "MUTE", true)
                .AddField("Note", "If you disagree with the action taken, please reply to this message to open a ModMail ticket.", false)
                .WithColor(Color.LightOrange)
                .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                .WithFooter("By joining /r/3DS, you agree that you have read our rules and that you will follow them.\r\nHowever, you have not, and this has led to a punishment.");

            //Send both
            try
            {
                punishment.notifMsgID = guildUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            }
            catch (AggregateException e)
            {
                e.Handle((x) =>
                {
                    if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                    {
                        responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification and won't be able to open a modmail.");
                        return true;
                    }

                    return false;
                });
            }

            punishment.notifMsgID = guildUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);

            punishments.punishmentList.Add(punishment);

            await SavePunishment(punishments);
        }

        private async Task HandleUnmuteCommand(SocketGuildUser guildUser, SocketGuild guild)
        {
            if (guildUser.TimedOutUntil == null || guildUser.TimedOutUntil < DateTimeOffset.Now)
            {
                var notmutedBuilder = new EmbedBuilder()
                    .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                    .WithTitle("__User not timed out!__")
                    .WithDescription($":x: `{guildUser.Username}` [{guildUser.Id}] is not timed out.")
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp();

                if (isSlashCommand) await RespondToSlashCommand(notmutedBuilder);
                else await RespondToTextCommand(notmutedBuilder);

                return;
            }
            else
            { 
                await guildUser.RemoveTimeOutAsync();
            }

            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            List<Punishment> reversedPunishments = new();

            foreach (var item in punishments.punishmentList)
            {
                reversedPunishments.Add(item);
            }
            reversedPunishments.Reverse();

            foreach (var reversedItem in reversedPunishments)
            {
                if (reversedItem.targetID == guildUser.Id && reversedItem.type == Punishment.Type.MUTE && reversedItem.active)
                {
                    foreach (var item in punishments.punishmentList)
                    {
                        if (item.punishmentID == reversedItem.punishmentID)
                        {
                            var responseBuilder = new EmbedBuilder()
                                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                                .WithTitle("__Unmute applied successfully__")
                                .WithDescription($":white_check_mark: `{guildUser.Username}` [{guildUser.Id}] has been unmuted, their punishment **#{item.punishmentID}** has been set to inactive.")
                                .WithColor(Color.Green)
                                .WithCurrentTimestamp();

                            var notifBuilder = new EmbedBuilder()
                                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                                .WithTitle("__You have been unmuted__")
                                .WithDescription($"Your mute **#{item.punishmentID}** with reason `{item.reason}` has been set to inactive.")
                                .WithColor(Color.Green)
                                .WithCurrentTimestamp();

                            try
                            {
                                await guildUser.SendMessageAsync(embed: notifBuilder.Build());
                            }
                            catch (AggregateException e)
                            {
                                e.Handle((x) =>
                                {
                                    if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                                    {
                                        responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification.");
                                        return true;
                                    }

                                    return false;
                                });
                            }

                            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
                            else await RespondToTextCommand(responseBuilder);

                            item.active = false;
                            await SavePunishment(punishments);
                            break;
                        }
                    }
                }
            }
        }

        private async Task HandleWarnCommand(SocketGuildUser guildUser, string reason, SocketGuild guild)
        {
            //Create Punishment in DB and save
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            punishments.punishmentIndex++;

            Punishment punishment = new();
            punishment.targetID = guildUser.Id;
            punishment.type = Punishment.Type.WARN;
            punishment.reason = reason;
            punishment.duration = "N/A";
            punishment.modID = _user.Id;
            punishment.punishmentID = punishments.punishmentIndex;
            punishment.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            punishment.active = true;

            //Create Moderator Log
            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle("__Warn applied successfully__")
                .WithDescription($":white_check_mark: `{guildUser.Username}` [{guildUser.Id}] has been warned for __{reason}__. #{punishment.punishmentID}")
                .WithColor(Color.LightOrange)
                .WithCurrentTimestamp();

            //Create User DM
            var warnBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle("**__Ooops...It looks like you have broken the rules of the server.__**")
                .WithDescription($"You have been warned for __{reason}__.")
                .AddField("Punishment ID", $"#{punishment.punishmentID}", true)
                .AddField("Punishent Type", "WARN", true)
                .AddField("Note", "This is just a warning, but if you keep breaking the rules, you may get further punishment. If you disagree with the action taken, please reply to this message to open a ModMail ticket. ", false)
                .WithColor(Color.LightOrange)
                .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                .WithFooter("By joining /r/3DS, you agree that you have read our rules and that you will follow them.\r\nHowever, you have not, and this has led to a punishment.");

            //Send both
            try
            {
                punishment.notifMsgID = guildUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            }
            catch (AggregateException e)
            {
                e.Handle((x) =>
                {
                    if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                    {
                        responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification and won't be able to open a modmail.");
                        return true;
                    }

                    return false;
                });
            }


            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);

            punishments.punishmentList.Add(punishment);

            await SavePunishment(punishments);
        }

        private async Task HandleUnwarnCommand(SocketGuildUser? guildUser, string by, string? valueID, SocketGuild guild)
        {
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            List<Punishment> reversedPunishments = new();

            foreach (var item in punishments.punishmentList)
            {
                reversedPunishments.Add(item);
            }
            reversedPunishments.Reverse();

            switch (by)
            {
                case "id":

                    foreach (var item in punishments.punishmentList)
                    {
                        if (item.punishmentID == ulong.Parse(valueID))
                        {
                            var user = guild.GetUser(item.targetID);

                            var responseBuilder = new EmbedBuilder()
                                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                                .WithTitle("__Unwarn applied successfully__")
                                .WithDescription($":white_check_mark: `{user.Username}` [{user.Id}] has been unwarned, their punishment **#{item.punishmentID}** has been set to inactive.")
                                .WithColor(Color.Green)
                                .WithCurrentTimestamp();

                            var notifBuilder = new EmbedBuilder()
                                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                                .WithTitle("__You have been unwarned__")
                                .WithDescription($"Your warn **#{item.punishmentID}** with reason `{item.reason}` has been set to inactive.")
                                .WithColor(Color.Green)
                                .WithCurrentTimestamp();

                            try
                            {
                                await user.SendMessageAsync(embed: notifBuilder.Build());
                            }
                            catch (AggregateException e)
                            {
                                e.Handle((x) =>
                                {
                                    if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                                    {
                                        responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification.");
                                        return true;
                                    }

                                    return false;
                                });
                            }

                            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
                            else await RespondToTextCommand(responseBuilder);

                            item.active = false;
                            await SavePunishment(punishments);
                            break;
                        }
                    }
                    break;
                case "user":

                    foreach (var reversedItem in reversedPunishments)
                    {
                        if (reversedItem.targetID == guildUser.Id && reversedItem.type == Punishment.Type.WARN && reversedItem.active)
                        {
                            foreach (var item in punishments.punishmentList)
                            {
                                if (item.punishmentID == reversedItem.punishmentID)
                                {
                                    var responseBuilder = new EmbedBuilder()
                                        .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                                        .WithTitle("__Unwarn applied successfully__")
                                        .WithDescription($":white_check_mark: `{guildUser.Username}` [{guildUser.Id}] has been unwarned, their punishment **#{item.punishmentID}** has been set to inactive.")
                                        .WithColor(Color.Green)
                                        .WithCurrentTimestamp();

                                    var notifBuilder = new EmbedBuilder()
                                        .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                                        .WithTitle("__You have been unwarned__")
                                        .WithDescription($"Your warn **#{item.punishmentID}** with reason `{item.reason}` has been set to inactive.")
                                        .WithColor(Color.Green)
                                        .WithCurrentTimestamp();

                                    try
                                    {
                                        await guildUser.SendMessageAsync(embed: notifBuilder.Build());
                                    }
                                    catch (AggregateException e)
                                    {
                                        e.Handle((x) =>
                                        {
                                            if (x is HttpException && ((HttpException)x).DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                                            {
                                                responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification.");
                                                return true;
                                            }

                                            return false;
                                        });
                                    }

                                    if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
                                    else await RespondToTextCommand(responseBuilder);

                                    item.active = false;
                                    await SavePunishment(punishments);
                                    return;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private async Task HandleGetpunishmentsCommand(SocketGuildUser? guildUser, string by, string? valueID, SocketGuild guild)
        {
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            List<Punishment> foundPunishments = new();

            List<Punishment> reversedPunishments = new();

            foreach (var item in punishments.punishmentList)
            {
                reversedPunishments.Add(item);
            }
            reversedPunishments.Reverse();

            switch (by)
            {
                case "id":

                    if (ulong.Parse(valueID) > punishments.punishmentIndex)
                    {
                        var notfoundEmbedBuilder = new EmbedBuilder()
                            .WithAuthor($"Punishment #{valueID}", guild.IconUrl)
                            .WithTitle($":prohibited: Punishment not found!")
                            .WithDescription($"Try with a different ID! The most recent punishment is #{punishments.punishmentIndex}")
                            .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756404158599210/red.jpg")
                            .WithFooter($"Requested by {_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                            .WithColor(Color.Red);

                        if (isSlashCommand) await RespondToSlashCommand(notfoundEmbedBuilder);
                        else await RespondToTextCommand(notfoundEmbedBuilder);
                    }

                    foreach (var item in punishments.punishmentList)
                    {
                        if (item.punishmentID == ulong.Parse(valueID.ToString()))
                        {
                            string typeText = getTypeTexts(item.type)[0];
                            string emote = getTypeTexts(item.type)[1];

                            var idEmbedBuilder = new EmbedBuilder()
                                .WithAuthor($"Punishment #{item.punishmentID}", guild.IconUrl)
                                .WithTitle($"{emote} {typeText} ")
                                .WithDescription($":clock8: <t:{item.timestamp}:f>\n:dart: <@{item.targetID}>\n:cop: <@{item.modID}>\n**Reason**:\n`{item.reason}`")
                                .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                                .WithFooter($"Requested by {_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                                .WithColor(Color.LightOrange);

                            if (isSlashCommand) await RespondToSlashCommand(idEmbedBuilder);
                            else await RespondToTextCommand(idEmbedBuilder);
                        }
                    }
                    break;
                case "moderator":
                    var valueMod = guildUser;
                    int modPunishmentCount = 0;

                    //start building the framework of the embed
                    var modEmbedBuilder = new EmbedBuilder()
                        .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                        .WithFooter($"Requested by {_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                        .WithColor(Color.LightOrange);

                    foreach (var item in reversedPunishments)
                    {
                        if (item.modID == valueMod.Id) modPunishmentCount++;

                        if (item.modID == valueMod.Id && modEmbedBuilder.Fields.Count < 6)
                        {
                            string typeText = getTypeTexts(item.type)[0];
                            string emote = getTypeTexts(item.type)[1];

                            modEmbedBuilder.AddField($"{emote} {typeText}", $":clock8: <t:{item.timestamp}:f>\n:dart: <@{item.targetID}>\n:hash: **#{item.punishmentID}**\n**Reason**:\n`{item.reason}`", inline: true);
                        }
                    }

                    modEmbedBuilder.WithAuthor($"{valueMod.Username} [{valueMod.Id}] ~ Moderation History ~ Total: {modPunishmentCount}", valueMod.GetAvatarUrl() ?? valueMod.GetDefaultAvatarUrl());

                    if (isSlashCommand) await RespondToSlashCommand(modEmbedBuilder);
                    else await RespondToTextCommand(modEmbedBuilder);
                    break;
                case "target":
                    var valueTarget = guildUser;

                    //start building the framework of the embed
                    var targetEmbedBuilder = new EmbedBuilder()
                        .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                        .WithFooter($"Requested by {_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                        .WithColor(Color.LightOrange);

                    //Find each matching punishment...
                    foreach (var item in punishments.punishmentList)
                    {
                        if (item.targetID == valueTarget.Id && item.active)
                        {
                            //...and put it in a list
                            foundPunishments.Add(item);
                        }
                    }

                    targetEmbedBuilder.WithAuthor($"{valueTarget.Username} [{valueTarget.Id}] ~ Punishment History ~ Total: {foundPunishments.Count}", valueTarget.GetAvatarUrl() ?? valueTarget.GetDefaultAvatarUrl());

                    //if theres <= 6 found punishments we can put them on one page...
                    if (foundPunishments.Count <= 6)
                    {
                        foreach (Punishment item in foundPunishments)
                        {
                            //get message and emote strings
                            string typeText = getTypeTexts(item.type)[0];
                            string emote = getTypeTexts(item.type)[1];

                            //add field for each
                            targetEmbedBuilder.AddField($"{emote} {typeText}", $":clock8: <t:{item.timestamp}:f>\n:cop: <@{item.modID}>\n:hash: **#{item.punishmentID}**\n**Reason**:\n`{item.reason}`", inline: true);

                        }

                        //send the embed including fields
                        if (isSlashCommand) await RespondToSlashCommand(targetEmbedBuilder);
                        else await RespondToTextCommand(targetEmbedBuilder);

                        foundPunishments.Clear();
                    }
                    else
                    {
                        //... if not, we will have to create a paginated view, by adding the six newest punishments, and removing them
                        for (int i = 0; i < 6; i++)
                        {
                            Punishment item = foundPunishments.LastOrDefault();

                            //get message and emote strings
                            string typeText = getTypeTexts(item.type)[0];
                            string emote = getTypeTexts(item.type)[1];

                            //add field for each punishment
                            targetEmbedBuilder.AddField($"{emote} {typeText}", $":clock8: <t:{item.timestamp}:f>\n:cop: <@{item.modID}>\n:hash: **#{item.punishmentID}**\n**Reason**:\n`{item.reason}`", inline: true);

                            //and remove the punishment from the list again
                            foundPunishments.Remove(item);
                        }
                        var componentBuilder = new ComponentBuilder()
                            .WithButton("Next", "next-button");

                        //send out the embed
                        if (isSlashCommand) await RespondToSlashCommand(targetEmbedBuilder);
                        else await RespondToTextCommand(targetEmbedBuilder);

                        //commandPass = command;
                    }
                    break;
            }
        }

        private async Task HandleRoleCommand(string action, SocketGuildUser guildUser, IRole role)
        {

            if (!userHasPerms)
            {
                EmbedBuilder noPermissionBuilder = new EmbedBuilder()
                    .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                    .WithTitle("__No permission!__")
                    .WithDescription($"You do not have access to the command `/role`")
                    .WithCurrentTimestamp()
                    .WithColor(Color.Red);

                await _command.RespondAsync(embed: noPermissionBuilder.Build(), ephemeral: true);

                return;
            }

            switch (action)
            {
                case "add":

                    if (role.Id == 259871228406267905)
                    {
                        EmbedBuilder badRoleBuilder = new EmbedBuilder()
                            .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                            .WithTitle("__No permission!__")
                            .WithDescription($"You cannot assign that role!")
                            .WithCurrentTimestamp()
                            .WithColor(Color.Red);

                        if (isSlashCommand) await RespondToSlashCommand(badRoleBuilder);
                        else await RespondToTextCommand(badRoleBuilder);

                        return;
                    }

                    if (guildUser.Roles.Contains(role))
                    {
                        EmbedBuilder badRoleBuilder = new EmbedBuilder()
                            .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                            .WithTitle("__Can't give role!__")
                            .WithDescription($"*{guildUser.Username} `[{guildUser.Id}]`* already has {role.Mention}. Try removing it first.")
                            .WithCurrentTimestamp()
                            .WithColor(Color.Red);

                        await _command.RespondAsync(embed: badRoleBuilder.Build(), ephemeral: true);

                        return;
                    }

                    try
                    {
                        await guildUser.AddRoleAsync(role);
                    }
                    catch (Exception ex)
                    {
                        var failBuilder = new EmbedBuilder()
                            .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                            .WithTitle($"Something went wrong!")
                            .WithDescription($"Couldnt give role {role.Mention} to *{guildUser.Username} `[{guildUser.Id}]`*.\n" +
                            $"Error: `{ex.Message}`")
                            .WithColor(Color.Red);

                        if (isSlashCommand) await RespondToSlashCommand(failBuilder);
                        else await RespondToTextCommand(failBuilder);

                        Console.WriteLine(ex.ToString());
                        return;
                    }

                    var addedBuilder = new EmbedBuilder()
                        .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                        .WithTitle($"Role added!")
                        .WithDescription($"You have successfully added {role.Mention} to *{guildUser.Username} `[{guildUser.Id}]`*.")
                        .WithColor(role.Color);

                    if (isSlashCommand) await RespondToSlashCommand(addedBuilder);
                    else await RespondToTextCommand(addedBuilder);

                    break;
                case "remove":

                    if (!guildUser.Roles.Contains(role))
                    {
                        EmbedBuilder badRoleBuilder = new EmbedBuilder()
                            .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                            .WithTitle("__Can't remove role!__")
                            .WithDescription($"*{guildUser.Username} `[{guildUser.Id}]`* does not have {role.Mention}. Try adding it first.")
                            .WithCurrentTimestamp()
                            .WithColor(Color.Red);

                        await _command.RespondAsync(embed: badRoleBuilder.Build(), ephemeral: true);

                        return;
                    }

                    try
                    {
                        await guildUser.RemoveRoleAsync(role);
                    }
                    catch (Exception ex)
                    {
                        var failBuilder = new EmbedBuilder()
                            .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                            .WithTitle($"Something went wrong!")
                            .WithDescription($"Couldnt remove {role.Mention} from *{guildUser.Username} `[{guildUser.Id}]`*.\n" +
                            $"Error: `{ex.Message}`")
                            .WithColor(Color.Red);

                        if (isSlashCommand) await RespondToSlashCommand(failBuilder);
                        else await RespondToTextCommand(failBuilder);

                        Console.WriteLine(ex.ToString());
                        return;
                    }

                    var removedBuilder = new EmbedBuilder()
                        .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                        .WithTitle($"Role removed!")
                        .WithDescription($"You have successfully removed {role.Mention} from *{guildUser.Username} `[{guildUser.Id}]`*.")
                        .WithColor(role.Color);

                    if (isSlashCommand) await RespondToSlashCommand(removedBuilder);
                    else await RespondToTextCommand(removedBuilder);

                    break;
            }



        }

        private async Task HandleDenyCommand(ulong userId, SocketGuild guild)
        {
            SocketUser user;
            user = guild.GetUser(userId);

            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle($"Application successfully denied!")
                .WithDescription($"You have successfully denied the application of {user.Username} `[{user.Id}]`. They will receive the unfortunate news in DMs.")
                .WithColor(Color.Green);

            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);

            var notifBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle($"__Your staff application__")
                .WithDescription($"Hey there! Thank you for applying. Our team has reviewed your application " +
                "& we regret to inform you that you were not chosen for moderator as you do not fulfill our requirements.\n\n" +
                "However we appreciate you taking interest & time to apply, your dedication is appreciated. We hope you continue to be a part of " +
                "& engage with our community.\n\n" +
                "You may reapply for the position once the next round of staff applications are announced, Good Luck!\n\n" +
                "Kind regards,\nStaff Team at r/3DS Discord")
                .WithColor(Color.Red)
                .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756404158599210/red.jpg")
                .WithFooter("Thank you for your interest in becoming a part of the team!");

            try
            {
                await user.SendMessageAsync(embed: notifBuilder.Build());
            }
            catch (HttpException e)
            {
                if (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the news.");
                }
            }

        }

        private async Task HandleAcceptCommand(ulong userId, SocketGuild guild)
        {
            SocketGuildUser user;
            user = guild.GetUser(userId);

            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle($"Application successfully accetped!")
                .WithDescription($"You have successfully accepted the application of {user.Username} `[{user.Id}]`. They will receive the great news in DMs.")
                .WithColor(Color.Green);

            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);

            var notifBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle($"__Your staff application__")
                .WithDescription($"Hey there! Thanks for applying! Our staff team has reviewed your application"+
                " & have determined that you fit our requirements for **Moderator**" +
                " *Woohoo!* <:honk:640354545461100606> Below are the next steps of the application process.\n\n" +
                "You've automatically been assigned the necessary roles to begin your staff training! " +
                "Head on over to [#discord-mod-talk](https://canary.discord.com/channels/248504507430993921/248509081789136896) to begin your staff journey!\n\n" +
                "Thank you again for taking the time to apply to become a member of our talented staff team.\n\n" +
                "Best regards,\nStaff Team at r/3DS Discord")
                .WithColor(Color.Green)
                .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756751249576006/green.jpg")
                .WithFooter("Thank you for your interest in becoming a part of the team!");

            try
            {
                await user.SendMessageAsync(embed: notifBuilder.Build());
            }
            catch (HttpException e)
            {
                if (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the news. But they will still be assigned the roles.");
                }
            }

            await user.AddRoleAsync(248505366239772682);
            await user.AddRoleAsync(1267031294030778368);
        }

        private async Task HandleSayCommand()
        {
            if(_user == null) return;
            if(_user.IsBot) return;

            var msg = _message;
            await _message.DeleteAsync();

            await msg.Channel.SendMessageAsync(msg.Content.Remove(0,5));
        }

        private async Task HandlePingCommand()
        {
            Ping ping = new Ping();
            List<long> pings = new List<long>();

            for (int i = 0; i < 4; i++)
            {
                PingReply reply = ping.Send("stockholm5485.discord.gg", 10000);
                pings.Add(reply.RoundtripTime);
            }

            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle(":ping_pong: Pong!")
                .WithDescription($"My current ping: **{Math.Truncate(pings.Average())}**")
                .WithFooter("Average of 5 pings to stockholm5485.discord.gg")
                .WithColor(Color.Green)
                .WithCurrentTimestamp();

            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);
        }

        private async Task HandleFormatCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle($"About SD-Card formatting")
                .WithDescription($"For general information, please check [the FAQ](https://discord.com/channels/248504507430993921/1270692745056485417/1271329343058214923)\n\n" +
                $"The 3DS family can only read SD-Cards if theyre formatted in **FAT32**.\n" +
                $"For cards __under__ **32GB** this can be achieved with any standard tool.\n" +
                $"For cards __above__ **32GB** on Windows you will need a special tool,\n which can be downloaded [here](http://ridgecrop.co.uk/index.htm?guiformat.htm).\n" +
                $"**64GB** cards need an **Allocation unit size** of __32KB/32768 bytes__,\n **128GB** need __64KB/65536 bytes__.\n" +
                $"Cards above **128GB** are __not__ recommended because of performance issues.");
            await RespondToInfoCommand(replyBuilder);
        }
        private async Task HandleSDCommand()
        {

            //The 3DS can use SD cards up to 2TB in size.However, using cards larger than 128GB is not recommended, as it tends to cause issues.
            //Any cards over 32GB will have to be formatted to FAT32 in a computer or hacked console before they can be used(use an allocation unit size
            //of 32KB / 32768 for 64GB cards and 64KB / 65536 for 128GB cards or larger).
            //Buy SD cards from reputable brands(SanDisk, Samsung, Kingston, etc.). Preferably, purchase cards from a brick and mortar store near you, but Amazon is okay
            //if you must purchase online.NEVER buy cards from AliExpress, Wish, eBay or other similar sites.
            //Speed is irrelevant for the 3DS - it is limited to Class 4(4MB / s) speeds.The only reason to buy a faster SD card is for faster data transfer to your computer.

            var replyBuilder = new EmbedBuilder()
                .WithTitle($"About SD-Cards")
                .WithDescription($"For general information, please check [the FAQ](https://discord.com/channels/248504507430993921/1270692745056485417/1271329343058214923)\n\n" +
                $"The 3DS family *can* take cards up to 2TB. However this is not recommended as you will run into issues with cards larger than 128GB.\n" +
                $"Cards **above 32GB** will have to be specially formatted. Consult `?formatting` for more information.\n" +
                $"Buy SD cards from reputable brands(SanDisk, Samsung, Kingston, etc...). Never buy used cards or cards from questionable sources like AliExpress or Wish.\n" +
                $"Card speed is irrelevant for the 3DS as it is limited to 4MB/s (Class 4). Faster speeds will only benefit you when transferring files from your PC to the card.");


            await RespondToInfoCommand(replyBuilder);
        }

        private async Task HandlePiracyCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle("About Piracy")
                .WithDescription("Piracy is **illegal** and against **Discord TOS**, so we do NOT allow any discussion of it.\n" +
                "We also can not help with troubleshooting pirated games.\n\n" +
                "Homebrew and 'hacking' does not automatically mean illegally downloading games or any other copyrighted content.\n" +
                "Piracy paints the homebrew community in a bad light in legislators and publishers eyes, and gives console makers more incentive to lock down their systems, making the jobs of volunteer " +
                "homebrew developers harder and harder.\n\n" +
                "Any discussion of piracy or mentioning/sharing links to sites/applications enabling it will be met with a warning, pushback will lead to harsher punishments.");

            await RespondToInfoCommand(replyBuilder);

        }

        private async Task HandleScreenCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle("About TN vs IPS panels")
                .WithDescription("In short, which type of screen your console has **does not matter** in 99% of situations.\n\n" +
                "TN panels only drawback to IPS is reduced colour accuracy at extreme viewing angles. You basically need to be looking at your 3DS from the side to be able to tell.\n" +
                "IPS panels also use slightly more power, reducing battery life.\n" +
                "Think about it this way: if you need to ask someone else what sort of panel your console has, does it really matter? You couldn't immediately tell and your gaming expirence has been just as good not knowing.\n\n" +
                "If you **really** need to know what panels your console has, you can check on [3DSident](https://github.com/joel16/3DSident/releases)");

            await RespondToInfoCommand(replyBuilder);

        }

        private async Task HandleCitraCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle("About Citra and emulation")
                .WithDescription("Since the lawsuit against Citra developers, Nintendo has been attacking communities providing support for Citra and other 3DS emulators.\n\n" +
                "We will **not help you with emulating the 3DS** on other devices, we only support hacking actual 3DS systems.\n");

            await RespondToInfoCommand(replyBuilder);

        }

        private async Task HandleGuideCommand(string section)
        {
            string title = "Oops!";
            string description = "Something went wrong!";
            Color color = Color.DarkerGrey;

            if (string.IsNullOrEmpty(section))
            {
                title = "About guides";
                description = "Please **only use [3ds.hacks.guide](https://3ds.hacks.guide/)** when hacking your system.\n\n" +
                $"__3ds.hacks.guide__ is always kept up to date by the community and is a constant that allows us to effectively help you when you stumble upon an issue, since we know what process you followed.\n" +
                $"Other written and video guides are often out of date, or provide spotty information so you are advised **against** using them. If you still decide to follow one, you are **on your own** as " +
                $"we will __not__ be able to offer help in case something goes wrong.";
                color = Color.Purple;
            }
            else
            {
                switch (section)
                {
                    case "transfer":
                        title = "Doing a system transfer";
                        description = "1) If the new console isn't hacked already, install CFW on the new console using [**the guide**](https://3ds.hacks.guide)\n" +
                            "2) Do a system transfer normally. Choose **'Don't use the guide'** then **'PC-based transfer'** if asked.\n" +
                            "3) On the new console, download [faketik](https://github.com/ihaveamac/faketik/releases/latest) and place `faketik.3dsx` in the `3ds` folder on your SD root.\n" +
                            "4) Launch the **Homebrew Launcher** on the new console. [Follow this](https://wiki.hacks.guide/wiki/3DS:Troubleshooting/manually_entering_homebrew_launcher) if you don't know how.\n" +
                            "5) Once you are in the Homebrew Launcher, run **faketik**.\n" +
                            "6) Your Homebrew apps should appear on the homescreen!\n\n" +
                            "*Taken from [the guides FAQ](https://3ds.hacks.guide/faq)*";
                        color = Color.Teal;
                        break;
                    case "cfwupdate":
                        title = "Updating Luma";
                        description = "To update your Luma installation,\n1) [Download Luma3DS](https://github.com/LumaTeam/Luma3DS/releases/latest)\n" +
                            "2) Insert your SD card into your computer.\n" +
                            "3) Copy `boot.3dsx` and `boot.firm` from the `.zip` to the root of your SD card.\n" +
                            "4) Reinsert the SD card into your console and power it up!\n\n" +
                            "*Taken from [the guide](https://3ds.hacks.guide/restoring-updating-cfw)*";
                        color = Color.Magenta;
                        break;
                    case "systemupdate":
                        title = "Updating your System";
                        description = "**If you plan on hacking your system**\n" +
                            "Currently **every system version** is **hackable**, though there might be **easier** methods **for older versions**.\n" +
                            "Check [the guide](https://3ds.hacks.guide/get-started) for the **available methods** for your systems version.\n\n" +
                            "**If your system is already hacked**\n" +
                            "It's advised to **wait a bit** to see if [Luma3DS](https://github.com/LumaTeam/Luma3DS/releases/latest) needs to be updated **before** you update your system.\n" +
                            "Though it is **unlikely** that a system update would break Luma.\n\n" +
                            "*Referencing [the guides FAQ](https://3ds.hacks.guide/faq)*";
                        color = Color.DarkTeal;
                        break;
                    case "regionchange":
                        title = "Changing your consoles region";
                        description = "If you have **Luma3DS** installed you can play out-of-region games (ex. **U**S games on **E**uropean consoles) without having to region change.\n" +
                            "But especially for **J**apanese consoles, where you can't set the UI language to english, region changing is needed.\n" +
                            "Region changing is an involved process- if you already have CFW, please follow [the guide](https://3ds.hacks.guide/region-changing)\n" +
                            "Otherwise you need to [hack your console first](https://3ds.hacks.guide)";
                        color = Color.LightOrange;
                        break;
                    default:
                        title = "About guides";
                        description = "Please **only use [3ds.hacks.guide](https://3ds.hacks.guide/)** when hacking your system.\n\n" +
                        $"__3ds.hacks.guide__ is always kept up to date by the community and is a constant that allows us to effectively help you when you stumble upon an issue, since we know what process you followed.\n" +
                        $"Other written and video guides are often out of date, or provide spotty information so you are advised **against** using them. If you still decide to follow one, you are **on your own** as " +
                        $"we will __not__ be able to offer help in case something goes wrong.";
                        color = Color.Purple;
                        break;
                }
            }


            var replyBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(color)
                .WithDescription(description);

            await RespondToInfoCommand(replyBuilder);

        }

        private async Task HandleDiffCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle("About 'New'3DS vs 'Old'3DS")
                .WithDescription("A detailed description of all the models can be found in the **[FAQ](https://canary.discord.com/channels/248504507430993921/1270692745056485417/1270702483966005290)**\n\n" +
                "Briefly explained, the **New 3DS** models have 6 times the CPU power, and double the RAM compared to 'Old' models. New models have **faster game load times**, " +
                "**face tracking** for a better 3D expirence and some **exclusive games** that use the new models ZL/ZR buttons and the 'C-Stick'. Noteworthy is that the Old 3DS uses **full sized** SD cards while " +
                "the new models use **microSD** cards.\n" +
                "You can also customize your New 3DS **non-XL** console with **faceplates** in different designs.");

            await RespondToInfoCommand(replyBuilder);

        }

        private async Task HandleCatCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle($"Heres your random cat {_message.Author.Username}!")
                .WithImageUrl($"{Cat.GetRandomCat().url}")
                .WithFooter("Powered by cataas.com");

            if (isSlashCommand) await RespondToSlashCommand(replyBuilder);
            else await RespondToTextCommand(replyBuilder);
        }

        private async Task HandleDogCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle($"Heres your random dog {_message.Author.Username}!")
                .WithImageUrl($"{Dog.GetRandomDog().message}")
                .WithFooter("Powered by dog.ceo");

            if (isSlashCommand) await RespondToSlashCommand(replyBuilder);
            else await RespondToTextCommand(replyBuilder);
        }

        private async Task HandleOtterCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle($"Heres your random otter {_message.Author.Username}!")
                .WithImageUrl($"https://vendell.online/img/otter/{Otter.GetRandomOtter()}")
                .WithFooter("Powered by vendell :)");

            if (isSlashCommand) await RespondToSlashCommand(replyBuilder);
            else await RespondToTextCommand(replyBuilder);
        }

        private async Task HandleBirdCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle($"Heres your random bird {_message.Author.Username}!")
                .WithImageUrl($"{Bird.GetRandomBird().image}")
                .WithFooter("Powered by some-random-api.com");

            if (isSlashCommand) await RespondToSlashCommand(replyBuilder);
            else await RespondToTextCommand(replyBuilder);

        }

        private async Task HandleIdiotCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle($"{_message.Author.Username} is an idiot!")
                .WithImageUrl($"https://cdn.discordapp.com/attachments/1227707463340523590/1363979968387874867/image.png")
                .WithFooter("hahahahahahahahahahaha");

            if (isSlashCommand) await RespondToSlashCommand(replyBuilder);
            else await RespondToTextCommand(replyBuilder);
        }

        public async Task HandleAuditLog(SocketAuditLogEntry logEntry, SocketGuild guild, DiscordSocketClient client)
        {
            if (logEntry.User.Id == 1244323092935872532)
            {
                return;
            }

            switch (logEntry.Action) 
            {
                case ActionType.Ban:
                    await HandleBanAuditLog(logEntry, guild, client);
                    break;
                case ActionType.Kick:
                    await HandleKickAuditLog(logEntry, guild, client);
                    break;
                default:
                    break;
            }
        }

        private async Task HandleBanAuditLog(SocketAuditLogEntry logEntry, SocketGuild guild, DiscordSocketClient client)
        {
            SocketBanAuditLogData data = logEntry.Data as SocketBanAuditLogData;
            ulong bannedUserID = data.Target.Id;
            SocketUser bannedUser = await client.GetUserAsync(bannedUserID) as SocketUser;

            _modChannel = guild.GetTextChannel(modChannelID);

            //Create Punishment in DB and save
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            punishments.punishmentIndex++;

            Punishment punishment = new();
            punishment.targetID = bannedUser.Id;
            punishment.type = Punishment.Type.BAN;
            punishment.reason = logEntry.Reason;
            punishment.duration = "N/A";
            punishment.modID = logEntry.User.Id;
            punishment.punishmentID = punishments.punishmentIndex;
            punishment.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            punishment.active = true;

            //await target.BanAsync(0, $"{reason} #{punishment.punishmentID}");

            //Create Mod Log
            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{logEntry.User.Username} [{logEntry.User.Id}]", logEntry.User.GetAvatarUrl() ?? logEntry.User.GetDefaultAvatarUrl())
                .WithTitle("__Ban applied successfully__")
                .WithDescription($":white_check_mark: `{bannedUser.Username}` [{bannedUser.Id}] has been banned for __{logEntry.Reason}__")
                .WithColor(Color.Red)
                .WithCurrentTimestamp();

            //Create User DM
            var warnBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle("**__Ooops...It looks like you've broken the rules of the server.__**")
                .WithDescription($"You have been banned for __{logEntry.Reason}__.")
                .AddField("Punishment ID", $"#{punishment.punishmentID}", true)
                .AddField("Punishent Type", "BAN", true)
                .AddField("Note", "If you disagree with the action taken, please visit [this link.](https://forms.gle/CMm8jPAxQCSoGYVY8)", false)
                .WithColor(Color.LightOrange)
                .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                .WithFooter("By joining /r/3DS, you agree that you have read our rules and that you will follow them.\r\nHowever, you have not, and this has led to a punishment.");

            //Send both
            punishment.notifMsgID = bannedUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            await _modChannel.SendMessageAsync(embed: responseBuilder.Build());

            punishments.punishmentList.Add(punishment);

            await SavePunishment(punishments);
        }

        private async Task HandleKickAuditLog(SocketAuditLogEntry logEntry, SocketGuild guild, DiscordSocketClient client)
        {
            SocketKickAuditLogData data = logEntry.Data as SocketKickAuditLogData;
            ulong kickedUserID = data.Target.Id;
            IUser kickedUser = await client.GetUserAsync(kickedUserID);

            _modChannel = guild.GetTextChannel(modChannelID);

            //Create Punishment in DB
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            punishments.punishmentIndex++;

            Punishment punishment = new();
            punishment.targetID = kickedUser.Id;
            punishment.type = Punishment.Type.KICK;
            punishment.reason = logEntry.Reason;
            punishment.duration = "N/A";
            punishment.modID = logEntry.User.Id;
            punishment.punishmentID = punishments.punishmentIndex;
            punishment.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            punishment.active = true;

            //await guildUser.KickAsync($"{reason} #{punishment.punishmentID}");

            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{logEntry.User.Username} [{logEntry.User.Id}]", logEntry.User.GetAvatarUrl() ?? logEntry.User.GetDefaultAvatarUrl())
                .WithTitle("__Kick applied successfully__")
                .WithDescription($":white_check_mark: `{kickedUser.Username}` [{kickedUser.Id}] has been kicked for __{logEntry.Reason}__")
                .WithColor(Color.Orange)
                .WithCurrentTimestamp();

            //Create User DM
            var warnBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle("**__Ooops...It looks like you've broken the rules of the server.__**")
                .WithDescription($"You have been kicked for __{logEntry.Reason}__.")
                .AddField("Punishment ID", $"#{punishment.punishmentID}", true)
                .AddField("Punishent Type", "KICK", true)
                .AddField("Note", "If you disagree with the action taken, please reply to this message to open a ModMail ticket.", false)
                .WithColor(Color.LightOrange)
                .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                .WithFooter("By joining /r/3DS, you agree that you have read our rules and that you will follow them.\r\nHowever, you have not, and this has led to a punishment.");

            //Send both and save notification message ID
            punishment.notifMsgID = kickedUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            await _modChannel.SendMessageAsync(embed: responseBuilder.Build());

            //save punishment in DB
            punishments.punishmentList.Add(punishment);

            await SavePunishment(punishments);
        }

        public static string[] getTypeTexts(Punishment.Type type)
        {
            string[] strings = { "", "" };

            switch (type)
            {
                case Punishment.Type.WARN:
                    strings[0] = "Warn";
                    strings[1] = ":warning:";
                    break;
                case Punishment.Type.MUTE:
                    strings[0] = "Mute";
                    strings[1] = ":mute:";
                    break;
                case Punishment.Type.KICK:
                    strings[0] = "Kick";
                    strings[1] = ":boot:";
                    break;
                case Punishment.Type.BAN:
                    strings[0] = "Ban";
                    strings[1] = ":hammer:";
                    break;
            }

            return strings;
        }

        public async Task SavePunishment(PunishmentFileRoot punishments)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(string.Concat(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "\\punishments.json"), JsonConvert.SerializeObject(punishments, Formatting.Indented));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                File.WriteAllText("/home/vendell/inet/punishments.json", JsonConvert.SerializeObject(punishments, Formatting.Indented));
            }
        }

        public async Task HandleUnknownCommand(string command)
        {

            var result = FuzzySharp.Process.ExtractOne(command, commands);

            string suggestion = "";

            if (result.Score > 75)
            {
                suggestion = $"\n:white_check_mark: Did you mean `{result.Value}`?";
            }

            var errorBuilder = new EmbedBuilder()
                .WithAuthor($"{_message.Author.Username} [{_message.Author.Id}]", _message.Author.GetAvatarUrl() ?? _message.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Oops...I'm Not Familiar With That Command!__")
                .WithDescription($":prohibited: You've entered an unknown command! Try **?help**{suggestion}")
                .WithColor(Color.Red)
                .WithCurrentTimestamp();

            await RespondToTextCommand(errorBuilder);
        }
    }
}
