using Discord;
using Discord.WebSocket;
using InetBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace InetBot.Modules
{
    internal class BanAppeals
    {
        public async Task HandleAppeal(Commands commands, SocketGuild guild, FormResponse formResponse)
        {
            EmbedBuilder appealBuilder = new EmbedBuilder()
                .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                .WithTitle("New ban appeal!")
                .WithDescription($"**Username:** {formResponse.username}\n" +
                $"**Punishment ID:** {formResponse.id}\n" +
                $"**Ban Reason:** {formResponse.reasonBan}\n" +
                $"**Unban Reason:** {formResponse.reasonUnban} \n" +
                $"**e-Mail Address:** {formResponse.email} \n")
                .WithFooter("Discuss!")
                .WithColor(Color.Gold);

            List<ButtonBuilder> buttons = new();

            ButtonBuilder acceptButton = new ButtonBuilder($"Accept", $"appeals-{formResponse.username}-{formResponse.email}-accept", ButtonStyle.Success);
            ButtonBuilder denyButton = new ButtonBuilder($"Deny", $"appeals-{formResponse.username}-{formResponse.email}-deny", ButtonStyle.Danger);

            buttons.Add(acceptButton);
            buttons.Add(denyButton);

            ActionRowBuilder rowBuilder = new ActionRowBuilder()
                .AddComponents(buttons.ToArray());

            var builder = new ComponentBuilderV2()
                .WithActionRow(rowBuilder);

            await guild.GetTextChannel(248509081789136896).SendMessageAsync(embed: appealBuilder.Build(), components:builder.Build());
        }

        public async Task AcceptAppeal(SocketMessageComponent component, SocketGuild guild)
        {
            string username = component.Data.CustomId.Split("-")[1];
            string email = component.Data.CustomId.Split("-")[2];

            await component.UpdateAsync(x =>
            {
                x.Content = ":white_check_mark: Accepted!";
                x.Components = null;
            });
            await SendMail("accept", username, email);
        }

        public async Task DenyAppeal(SocketMessageComponent component, SocketGuild guild)
        {
            string username = component.Data.CustomId.Split("-")[1];
            string email = component.Data.CustomId.Split("-")[2];

            await component.UpdateAsync(x =>
            {
                x.Content = ":no_entry_sign: Denied!";
                x.Components = null;
            });
            await SendMail("deny", username, email);
        }

        async Task SendMail(string action, string username, string email)
        {
            string acceptMessage = $"Dear {username}!\n\n" +
                $"We have reviewed your ban appeal and have decided to accept it. You may now rejoin the server.\n\n" +
                $"Kindest regards,\n" +
                $"r/3DS Discord Moderation Team\n\n" +
                $"Please keep in mind that this inbox is not monitored, and replies are not read.";

            string denyMessage = $"Dear {username}!\n\n" +
                $"We have reviewed your ban appeal and have decided to deny it.\n\n" +
                $"Kindest regards,\n" +
                $"r/3DS Discord Moderation Team\n\n" +
                $"Please keep in mind that this inbox is not monitored, and replies are not read.";

            string body = "";

            if (action == "accept") body = acceptMessage;
            if (action == "deny") body = denyMessage;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("r/3DS Modteam", "modteam@3ds.vendell.online"));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = "Your r/3DS Discord Ban Appeal";


            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.zoho.eu", 587, false);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(BotToken.smtpUser, BotToken.smtpPass);

                client.Send(message);
                client.Disconnect(true);
            }
        }

    }
}
