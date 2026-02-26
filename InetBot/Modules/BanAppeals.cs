using Discord;
using Discord.WebSocket;
using InetBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            await guild.GetTextChannel(248509081789136896).SendMessageAsync(embed: appealBuilder.Build());
        }
    }
}
