using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace linkusBot.Modules
{
    public class Reactions
    {
        SocketGuild guild;

        public async Task HandleReaction(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction, SocketGuild _guild)
        {
            guild = _guild;

            switch (message.Id) 
            {
                case 1260477725118824478:
                    await HandleVerification(reaction);
                    break;
            }
        }

        private async Task HandleVerification(SocketReaction react)
        {
            if (react.Emote.Name == "o3ds")
            {
                SocketGuildUser guildUser = react.User.Value as SocketGuildUser;
                await guildUser.AddRoleAsync(1258397995070652548);

                var notifEmbed = new EmbedBuilder()
                    .WithAuthor($"{guild.Name} [{guild.Id}]", guild.IconUrl)
                    .WithTitle($"__Verification successful!__")
                    .WithDescription("You have successfully verified yourself in r/3DS! We hope you enjoy your stay!")
                    .WithColor(Color.Green)
                    .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1261325580272664586/accept.png")
                    .WithFooter("You agree to have read and abide by the rules of the r/3DS Discord.\nFailure to do so will result in punishment.");

                await guildUser.SendMessageAsync(embed: notifEmbed.Build());
            }
        }
    }
}
