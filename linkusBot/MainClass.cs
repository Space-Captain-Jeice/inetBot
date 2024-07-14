using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Discord.Net;
using Newtonsoft.Json;
using linkusBot.Modules;
using linkusBot.Data;
using System.Security;

namespace linkusBot
{
    public class MainClass
    {
        private static DiscordSocketClient _client;

        //we pass these to other methods
        private SocketGuild _guild;
        private SocketTextChannel _modChannel;



        public async Task Run()
        {
            DiscordSocketConfig config = new();
            config.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent;
            config.AlwaysDownloadUsers = true;

            _client = new DiscordSocketClient(config);

            _client.Log += Log;
            _client.Ready += Client_Ready;

            await _client.SetGameAsync("Waiting to listen in /r/3DS", null, ActivityType.CustomStatus);

            _client.MessageReceived += MessageRecievedHandler;
            _client.AuditLogCreated += AuditLogCreated;

            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.ButtonExecuted += ButtonHandler;
            _client.ReactionAdded += ReactionHandler;

            var token = BotToken.token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task AuditLogCreated(SocketAuditLogEntry logEntry, SocketGuild guild)
        {
            Commands commands = new Commands();
            await commands.HandleAuditLog(logEntry, guild);
        }

        private async Task Client_Ready()
        {
            _guild = _client.GetGuild(1244328365129994240);
            _modChannel = _guild.GetTextChannel(1244346391086764124);

            Console.WriteLine("jorking my peanits");

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

                //await _guild.CreateApplicationCommandAsync(getpunishmentsCommand.Build());

                //await _guild.CreateApplicationCommandAsync(helpCommand.Build());
            }
            catch (ApplicationCommandException ex)
            {

                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }
        }
        private async Task ReactionHandler(Cacheable<IUserMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2, SocketReaction reaction)
        {
            Reactions reactions = new Reactions();
            await reactions.HandleReaction(cacheable1, cacheable2, reaction, _guild);
        }

        private async Task MessageRecievedHandler(SocketMessage message)
        {
            ModMail modMail = new();
            //TextCommands textCommands = new();
            Commands commands = new Commands();

            if (message.Content.StartsWith("?"))
            {
                //await textCommands.HandleCommand(message, _guild, _client);
                await commands.HandleCommand(message, _guild);
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

            switch (component.Data.CustomId)
            {
                case "punishment-next-button":
                    await buttons.PunishmentNextButton(component);
                    break;
                    
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            Commands commands = new();
            await commands.HandleCommand(command, _guild);
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
