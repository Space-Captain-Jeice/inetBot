using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Google.Apis.Forms.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetBot.Modules
{
    internal class Buttons
    {
        public async Task HandlePunishmentNextButton(SocketMessageComponent component, SocketGuild guild)
        {
            Commands commands = new();
            await commands.GetModChannel(guild);

            //"punishment-next-{by}-{guildUser.Id}-{page+1}"
            string[] args = component.Data.CustomId.Split("-");

            commands._user = component.User;
            await commands.HandleGetpunishmentsCommand(guild.GetUser(ulong.Parse(args[3])), args[2], null, guild, int.Parse(args[4]));
            
            await component.RespondAsync(embed:commands.returnEmbedBuilder.Build(), components: commands.returnComponentBuilder.Build(), ephemeral: true);
        }

        public async Task HandlePunishmentShareButton(SocketMessageComponent component)
        {
            await component.RespondAsync(embed: component.Message.Embeds.FirstOrDefault());
        }
    }
}
