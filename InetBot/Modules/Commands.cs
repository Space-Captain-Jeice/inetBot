using Discord;
using Discord.Net;
using Discord.WebSocket;
using InetBot.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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

        string[] modCommands = ["ban", "unban", "kick", "unkick", "mute", "warn", "unwarn", "getpunishments", "accept", "deny"];
        SocketTextChannel _modChannel;

        private ulong modChannelID = 440118112977944578;

        //
        // Summary:
        //     Handle a SocketSlashCommand.
        public async Task HandleCommand(SocketSlashCommand command, SocketGuild guild)
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
                if (item.Id == 455414864056156170) userHasPerms = true;
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

                    await HandleUnbanCommand(guildUserId, guild);
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
        public async Task HandleCommand(SocketMessage message, SocketGuild guild)
        {
            _message = message;
            _user = message.Author;
            _userMessage = message as SocketUserMessage;

            string msg = message.Content.Remove(0, 1);
            string cmd = msg.Split(" ")[0].ToLower();

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
                if (item.Id == 455414864056156170) userHasPerms = true;
            }

            EmbedBuilder noPermissionBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle("__No permission!__")
                .WithDescription($"You do not have access to the command `?{cmd}`")
                .WithColor(Color.Red);

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
                    await HandleUnbanCommand(guildUserId, guild);
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
                case "cat":
                    await HandleCatCommand(); 
                    break;
                case "dog":
                    await HandleDogCommand();
                    break;
                case "otter":
                    await HandleOtterCommand();
                    break;
                default:
                    await HandleUnknownCommand();
                    break;
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

        private async Task HandleHelpCommand(SocketGuildUser guildUser)
        {
            if (userHasPerms)
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
                "`?otter/dog/cat`\n" + 
                "Gets a random image of your favourite critter.\n");

            if (isSlashCommand) await RespondToSlashCommand(userReplyBuilder);
            else await RespondToTextCommand(userReplyBuilder);
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
            catch (HttpException e)
            {
                if (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not recieve the appeal form.");
                }
            }

            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);

            punishments.punishmentList.Add(punishment);

            await SavePunishment(punishments);

            //await target.BanAsync(0, $"{reason} #{punishment.punishmentID}");
        }

        private async Task HandleUnbanCommand(ulong guildUserId, SocketGuild guild)
        {
            //await guild.RemoveBanAsync(guildUserId);

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
                            var user = guild.GetUser(guildUserId);

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

                            await user.SendMessageAsync(embed: notifBuilder.Build());

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
            catch (HttpException e)
            {
                if (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not recive the notification and won't be able to open a modmail.");
                }
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

                            await user.SendMessageAsync(embed: notifBuilder.Build());
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

                                    await guildUser.SendMessageAsync(embed: notifBuilder.Build());
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

            await guildUser.SetTimeOutAsync(new TimeSpan(int.Parse(days), int.Parse(hours), int.Parse(minutes), 0));

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
            catch (HttpException e)
            {
                if (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not recive the notification and won't be able to open a modmail.");
                }
            }

            punishment.notifMsgID = guildUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            if (isSlashCommand) await RespondToSlashCommand(responseBuilder);
            else await RespondToTextCommand(responseBuilder);

            punishments.punishmentList.Add(punishment);

            await SavePunishment(punishments);
        }

        private async Task HandleUnmuteCommand(SocketGuildUser guildUser, SocketGuild guild)
        {
            //await guildUser.RemoveTimeOutAsync();

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

                            await guildUser.SendMessageAsync(embed: notifBuilder.Build());
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
                .AddField("Note", "This is just a warning, but if you keep breaking the rules, you may get further punishment. If you disagree with the action taken, please visit [this link.](https://docs.google.com/forms/d/16KdS0jBFY79g0rOOCmTS5qZ9_WLNzQqNOzmWrUbmwyU)", false)
                .WithColor(Color.LightOrange)
                .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                .WithFooter("By joining /r/3DS, you agree that you have read our rules and that you will follow them.\r\nHowever, you have not, and this has led to a punishment.");

            //Send both
            try
            {
                punishment.notifMsgID = guildUser.SendMessageAsync(embed: warnBuilder.Build()).Result.Id;
            }
            catch (HttpException e)
            {
                if (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not recive the notification and won't be able to open a modmail.");
                }
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

                            await user.SendMessageAsync(embed: notifBuilder.Build());
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

                                    await guildUser.SendMessageAsync(embed: notifBuilder.Build());
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
                                .WithDescription($":clock8: <t:{item.timestamp}:f>\n:dart: <@{item.targetID}>\n**Reason**:\n`{item.reason}`")
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

                    var modEmbedBuilder = new EmbedBuilder()
                        .WithAuthor($"{valueMod.Username} [{valueMod.Id}] ~ Moderation History", valueMod.GetAvatarUrl() ?? valueMod.GetDefaultAvatarUrl())
                        .WithImageUrl("https://cdn.discordapp.com/attachments/971110878638407764/1244388234981670995/lightOrange.jpg")
                        .WithFooter($"Requested by {_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                        .WithColor(Color.LightOrange);

                    foreach (var item in reversedPunishments)
                    {
                        if (item.modID == valueMod.Id && modEmbedBuilder.Fields.Count < 6)
                        {
                            string typeText = getTypeTexts(item.type)[0];
                            string emote = getTypeTexts(item.type)[1];

                            modEmbedBuilder.AddField($"{emote} {typeText}", $":clock8: <t:{item.timestamp}:f>\n:dart: <@{item.targetID}>\n:hash: **#{item.punishmentID}**\n**Reason**:\n`{item.reason}`", inline: true);
                        }
                    }

                    if (isSlashCommand) await RespondToSlashCommand(modEmbedBuilder);
                    else await RespondToTextCommand(modEmbedBuilder);
                    break;
                case "target":
                    var valueTarget = guildUser;

                    //start building the framework of the embed
                    var targetEmbedBuilder = new EmbedBuilder()
                        .WithAuthor($"{valueTarget.Username} [{valueTarget.Id}] ~ Punishment History", valueTarget.GetAvatarUrl() ?? valueTarget.GetDefaultAvatarUrl())
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

        private async Task HandleDenyCommand(ulong userId, SocketGuild guild)
        {
            SocketUser user;
            user = guild.GetUser(userId);

            var responseBuilder = new EmbedBuilder()
                .WithAuthor($"{_user.Username} [{_user.Id}]", _user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                .WithTitle($"Application successfully denied!")
                .WithDescription($"You have successfully denied the application of {user.Username} `[{user.Id}]`. They will recieve the unfortunate news in DMs.")
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
                    responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not recieve the news.");
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
                .WithDescription($"You have successfully accepted the application of {user.Username} `[{user.Id}]`. They will recieve the great news in DMs.")
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
                    responseBuilder.AddField("Note!", "I couldn't send the user a DM. They will not recieve the news. But they will still be assigned the roles.");
                }
            }

            await user.AddRoleAsync(1252237002707963977);
            await user.AddRoleAsync(1258719839946801212);
        }

        private async Task HandleCatCommand()
        {
            var replyBuilder = new EmbedBuilder()
                .WithTitle($"Heres your random cat {_message.Author.Username}!")
                .WithImageUrl($"https://cataas.com/cat/{Cat.GetRandomCat()._id}")
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
                .WithFooter("Powered by vendell.online");

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
                File.WriteAllText(string.Concat(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "\\punishments.json"), JsonConvert.SerializeObject(punishments, Formatting.Indented));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                File.WriteAllText("/home/vendell/inet/punishments.json", JsonConvert.SerializeObject(punishments, Formatting.Indented));
            }
        }

        public async Task HandleUnknownCommand()
        {
            var errorBuilder = new EmbedBuilder()
                .WithAuthor($"{_message.Author.Username} [{_message.Author.Id}]", _message.Author.GetAvatarUrl() ?? _message.Author.GetDefaultAvatarUrl())
                .WithTitle("__Oops...I'm Not Familiar With That Command!__")
                .WithDescription($":prohibited: You've entered an unknown command! Try **?help**")
                .WithColor(Color.Red)
                .WithCurrentTimestamp();

            await RespondToTextCommand(errorBuilder);
        }
    }
}
