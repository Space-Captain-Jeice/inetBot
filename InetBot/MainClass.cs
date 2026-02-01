using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Google.Apis.Forms.v1.Data;
using InetBot.Data;
using InetBot.Modules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Object = System.Object;

namespace InetBot
{
    public class MainClass
    {
        private static DiscordSocketClient _client;

        //we pass these to other methods
        private SocketGuild _guild;

        //3ds
        private ulong _guildId = 248504507430993921;
        //tsd
        //private ulong _guildId = 421017607710441492;

        private static System.Timers.Timer activityTimer = new();
        private int _activityCount = 0;

        private static System.Timers.Timer appealTimer = new();

        IAudioClient audioClient;

        ulong dummie = 0;
        ulong ultraDummie = 0;

        public async Task Run()
        {
            DiscordSocketConfig config = new();
            config.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent;
            config.AlwaysDownloadUsers = true;

            _client = new DiscordSocketClient(config);

            _client.Log += Log;
            _client.Ready += Client_Ready;

            await _client.SetGameAsync("Waiting for ?help in /r/3DS", null, ActivityType.CustomStatus);

            _client.MessageReceived += MessageRecievedHandler;
            _client.AuditLogCreated += AuditLogCreated;
            _client.AutoModActionExecuted += AutoModActionExecuted;

            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.ButtonExecuted += ButtonHandler;
            _client.ReactionAdded += ReactionHandler;
            _client.UserJoined += JoinHandler;

            var token = BotToken.token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task AutoModActionExecuted(SocketGuild guild, AutoModRuleAction action, AutoModActionExecutedData data)
        {
            Commands commands = new Commands();
            commands._user = _client.CurrentUser;
            commands.isSlashCommand = false;
            commands._modChannel = guild.GetTextChannel(commands.modChannelID);

            if (data.AlertMessageId != 0) return;

            //3ds
            if (data.Rule.Id == 976298046214266890)
            //tsd
            //if (data.Rule.Id == 1357104233262088202)
            {

                if (data.User.Value.Id == dummie)
                {
                    if (dummie == ultraDummie)
                    {
                        await commands.HandleWarnCommand(data.User.Value, "Repeated piracy (AutoMod)", guild);
                        return;
                    }

                    EmbedBuilder dummieBuilder = new EmbedBuilder()
                        .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl).WithTitle("__AutoMod triggered!__")
                        .WithDescription($"You are trying to send a message containing a piracy word, which breaks the rules of the server.\n\n" +
                        $"☠ **While homebrew and flashcart discussion is allowed, talk about piracy or links that redirect to ROM/emulator download sites is strictly prohibited. It's illegal and can lead to all sorts of trouble, simple as that.**\n\n")
                        .WithFooter("This is an automated message. It is merely informative and no action has been taken.")
                        .WithColor(Color.Red);

                    EmbedBuilder dummieNotifBuilder = new EmbedBuilder()
                        .WithAuthor($"{data.User.Value.Username} [{data.User.Value.Id}]", data.User.Value.GetAvatarUrl() ?? data.User.Value.GetDefaultAvatarUrl()).WithTitle("__AutoMod triggered!__")
                        .WithDescription($"`{data.User.Value.Username}` has been notified of the piracy rule!")
                        .WithColor(Color.Red);

                    await data.User.Value.SendMessageAsync(embed: dummieBuilder.Build());
                    await guild.GetTextChannel(commands.modChannelID).SendMessageAsync(embed:dummieNotifBuilder.Build());
                    ultraDummie = dummie;
                    dummie = 0;
                }
                else
                {
                    dummie = data.User.Value.Id;
                }
            }
        }

        private async Task AuditLogCreated(SocketAuditLogEntry logEntry, SocketGuild guild)
        {
            Commands commands = new Commands();
            //await commands.HandleAuditLog(logEntry, guild, _client);
        }

        private async Task JoinHandler(SocketGuildUser guildUser)
        {
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();
            List<Punishment> reversedPunishments = new();

            foreach (var item in punishments.punishmentList)
            {
                reversedPunishments.Add(item);
            }
            reversedPunishments.Reverse();

            foreach (var reversedItem in reversedPunishments)
            {
                if (reversedItem.targetID == guildUser.Id && reversedItem.type == Punishment.Type.NOHELP && reversedItem.active)
                {
                    await guildUser.AddRoleAsync(1394395701076557844);
                }
            }
        }

        private async void SetActivity(Object source, ElapsedEventArgs e)
        {
            switch (_activityCount)
            {
                case 0:
                    await _client.SetGameAsync("DM me for Modmail!", null, ActivityType.CustomStatus);
                    _activityCount++;
                    break;
                case 1:
                    await _client.SetGameAsync("Waiting for ?help in /r/3DS", null, ActivityType.CustomStatus);
                    _activityCount++;
                    break;
                case 2:
                    await _client.SetGameAsync("Fly high Gizmo <3", null, ActivityType.CustomStatus);
                    _activityCount++;
                    break;
                case 3:
                    await _client.SetGameAsync("Watching D3R-B0T", null, ActivityType.CustomStatus);
                    _activityCount = 0;
                    break;

            }
        }

        private static void tcpListen()
        {
            int port = 46672;

            TcpListener server = new TcpListener(IPAddress.Any, port);

            server.Start();

            while (true)
            {
                using TcpClient client = server.AcceptTcpClient();
            }
        }


        private async Task Client_Ready()
        {
            _guild = _client.GetGuild(_guildId);
            //await _guild.GetTextChannel(259887245324976148).GetMessageAsync(1358429192265535740).Result.Author.SendMessageAsync("hi :)\ndid you mean to message the moderators?");

            activityTimer.Interval = 30000;
            activityTimer.Elapsed += SetActivity;
            activityTimer.AutoReset = true;
            activityTimer.Enabled = true;

            BanAppeals appeals = new BanAppeals();
            //appeals.CheckAppeals();
            //appealTimer.Elapsed += appeals.CheckAppeals;

            Thread thread = new Thread(new ThreadStart(tcpListen));
            thread.Start();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Client Ready!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Guild: ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(_guild.Name + "\n");
            Console.ResetColor();

            //Thread songThread = new Thread(new ThreadStart(startStream));
            //songThread.Start();

            //var emote = Emote.Parse("<:o3ds:1261080733913710633>");
            //await _guild.GetTextChannel(1244346826174369862).GetMessageAsync(1260477725118824478).Result.AddReactionAsync(emote);

            #region COMMAND BUILDERS

            var kickCommand = new SlashCommandBuilder()
            .WithName("kick")
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithDescription("Kicks user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The user who you want to kick", isRequired: true)
            .AddOption("reason", ApplicationCommandOptionType.String, "The reason for the kick", isRequired: true);

            var unkickCommand = new SlashCommandBuilder()
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithName("unkicks")
            .WithDescription("Unkicks user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("id")
                .WithDescription("Undo kick by ID.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("id", ApplicationCommandOptionType.String, "The ID of the kick you want to undo.", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("Undo LATEST kick of a user.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("user", ApplicationCommandOptionType.User, "The user whose LATEST kick you want to undo.", isRequired: true));

            var banCommand = new SlashCommandBuilder()
            .WithName("ban")
            .WithDefaultMemberPermissions(GuildPermission.BanMembers)
            .WithDescription("Bans user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The user who you want to ban", isRequired: true)
            .AddOption("reason", ApplicationCommandOptionType.String, "The reason for the ban", isRequired: true);

            var unbanCommand = new SlashCommandBuilder()
            .WithName("unban")
            .WithDefaultMemberPermissions(GuildPermission.BanMembers)
            .WithDescription("Unbans user.")
            .AddOption("userid", ApplicationCommandOptionType.String, "The users ID who you want to unban", isRequired: true);

            var warnCommand = new SlashCommandBuilder()
            .WithName("warn")
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithDescription("Warns a user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The user who you want to warn", isRequired: true)
            .AddOption("reason", ApplicationCommandOptionType.String, "The warning message", isRequired: true);

            var unwarnCommand = new SlashCommandBuilder()
            .WithName("unwarn")
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithDescription("Unwarns user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("id")
                .WithDescription("Undo warning by ID.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("id", ApplicationCommandOptionType.String, "The ID of the warning you want to undo.", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("Undo LATEST warning of a user.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("user", ApplicationCommandOptionType.User, "The user whose LATEST warning you want to undo.", isRequired: true));

            var muteCommand = new SlashCommandBuilder()
            .WithName("mute")
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithDescription("Mutes a user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The user who you want to mute", isRequired: true)
            .AddOption("duration", ApplicationCommandOptionType.String, "The duration", isRequired: true)
            .AddOption("reason", ApplicationCommandOptionType.String, "The reason for the mute", isRequired: true);

            var unmuteCommand = new SlashCommandBuilder()
            .WithName("unmute")
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithDescription("Unmutes user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The user who you want to unmute", isRequired: true);

            var nohelpCommand = new SlashCommandBuilder()
            .WithName("nohelp")
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithDescription("Gives user the No Help role, removing their ability to post in #hacking and #questions-and-support.")
            .AddOption("user", ApplicationCommandOptionType.User, "The user who you want to nohelp", isRequired: true);

            var yeshelpCommand = new SlashCommandBuilder()
            .WithName("yeshelp")
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithDescription("Removes the No Help role from the user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The user who you want to yeshelp", isRequired: true);

            var getpunishmentsCommand = new SlashCommandBuilder()
            .WithName("getpunishments")
            .WithDefaultMemberPermissions(GuildPermission.KickMembers)
            .WithDescription("Gets a punishment by ID or list of punishments by target user or moderator.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("id")
                .WithDescription("The punishments ID")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("id", ApplicationCommandOptionType.String, "ID", isRequired: true)
                )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("moderator")
                .WithDescription("The acting moderator")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("mod", ApplicationCommandOptionType.User, "user", isRequired: true)
                )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("target")
                .WithDescription("The target")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("target", ApplicationCommandOptionType.User, "target", isRequired: true)
                );

            var roleCommand = new SlashCommandBuilder()
            .WithName("role")
            .WithDefaultMemberPermissions(GuildPermission.BanMembers)
            .WithDescription("Add or remove a role to/from a member. Only usable by Head Moderators and Admins")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("add")
                .WithDescription("Add a role.")
                .WithType (ApplicationCommandOptionType.SubCommand)
                .AddOption("target", ApplicationCommandOptionType.User, "The user who you want to add a role to.", isRequired: true)
                .AddOption("role", ApplicationCommandOptionType.Role, "The role you want to add.", isRequired: true)
                )
            .AddOption(new SlashCommandOptionBuilder ()
                .WithName("remove")
                .WithDescription("Remove a role.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("target", ApplicationCommandOptionType.User, "The user who you want to remove a role from.", isRequired: true)
                .AddOption("role", ApplicationCommandOptionType.Role, "The role you want to remove.", isRequired: true)
                );

            var acceptCommand = new SlashCommandBuilder()
            .WithName("accept")
            .WithDefaultMemberPermissions(GuildPermission.ManageRoles)
            .WithDescription("Accept a staff application.")
            .AddOption("user", ApplicationCommandOptionType.String, "The users ID.", isRequired: true);

            var denyCommand = new SlashCommandBuilder()
            .WithName("deny")
            .WithDefaultMemberPermissions(GuildPermission.ManageRoles)
            .WithDescription("Deny a staff application.")
            .AddOption("user", ApplicationCommandOptionType.String, "The users ID.", isRequired: true);

            var helpCommand = new SlashCommandBuilder()
            .WithName("help")
            .WithDescription("Shows a help message.");

            #endregion

            try
            {
                //await _guild.CreateApplicationCommandAsync(kickCommand.Build());
                //await _guild.CreateApplicationCommandAsync(unkickCommand.Build());

                //await _guild.CreateApplicationCommandAsync(banCommand.Build());
                //await _guild.CreateApplicationCommandAsync(unbanCommand.Build());

                //await _guild.CreateApplicationCommandAsync(warnCommand.Build());
                //await _guild.CreateApplicationCommandAsync(unwarnCommand.Build());

                //await _guild.CreateApplicationCommandAsync(muteCommand.Build());
                //await _guild.CreateApplicationCommandAsync(unmuteCommand.Build());

                //await _guild.CreateApplicationCommandAsync(nohelpCommand.Build());
                //await _guild.CreateApplicationCommandAsync(yeshelpCommand.Build());

                //await _guild.CreateApplicationCommandAsync(getpunishmentsCommand.Build());

                //await _guild.CreateApplicationCommandAsync(acceptCommand.Build());
                //await _guild.CreateApplicationCommandAsync(denyCommand.Build());

                //await _guild.CreateApplicationCommandAsync(helpCommand.Build());

                //await _guild.CreateApplicationCommandAsync(roleCommand.Build());
            }
            catch (ApplicationCommandException ex)
            {

                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }
        }

        private async void startStream()
        {
            audioClient = await _client.GetGuild(248504507430993921).GetVoiceChannel(248508901216092160).ConnectAsync();
            await SendAsync(audioClient, "mariah.wav");
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }

        private Process? CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -stream_loop -1 -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task ReactionHandler(Cacheable<IUserMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2, SocketReaction reaction)
        {
            Reactions reactions = new Reactions();
            //await reactions.HandleReaction(cacheable1, cacheable2, reaction, _guild);
        }

        private async Task MessageRecievedHandler(SocketMessage message)
        {
            ModMail modMail = new();
            //TextCommands textCommands = new();
            Commands commands = new Commands();

            //if (message.Channel.Id == 259878856507392001 && message.Author.Id == 271382258525405184 && message.Embeds.FirstOrDefault().Author.Value.Name.Contains("Inet-kun"))
            //{
            //    message.DeleteAsync();
            //}

            if (message.Content.StartsWith("?"))
            {
                //await textCommands.HandleCommand(message, _guild, _client);
                await commands.HandleCommand(message, _guild, _client);
            }
            else
            {
                //we're sending every message event to the modmail handler
                //is this bad? eh
                await modMail.HandleModMailMessage(message, _guild, _client);
            }
        }

        private async Task ButtonHandler(SocketMessageComponent component)
        {
            Buttons buttons = new();

            string customId = component.Data.CustomId;

            if (customId.StartsWith("punishment-next"))
            {
                string[] args = customId.Split('-');

                string by = args[2];
                ulong guildUserId = ulong.Parse(args[3]);
                int page = int.Parse(args[4]);

                Console.WriteLine($"{by} {guildUserId} {page}");

                await buttons.HandlePunishmentNextButton(component, _guild);

            }
            else if (customId.StartsWith("punishment-share"))
            {
                await buttons.HandlePunishmentShareButton(component);
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            Commands commands = new();
            await commands.HandleCommand(command, _guild, _client);
        }

        private Task Log(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }
    }
}
