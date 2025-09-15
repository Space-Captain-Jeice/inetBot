using Discord;
using Discord.Net;
using Discord.WebSocket;
using InetBot.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace InetBot.Modules
{
    internal class ModMail
    {
        SocketMessage sourceMessage;
        SocketGuild sourceGuild;
        DiscordSocketClient _client;

        ModMailTicketFileRoot ticketFileRoot;
        List<ModMailTicket> reversedModMailTickets = new List<ModMailTicket>();

        SocketRole modmailRole;

        //3ds
        ulong modmailChannelId = 1141532366142189620;
        //tsd
        //ulong modmailChannelId = 440118112977944578;

        //ulong modmailRoleId = 455414864056156170;


        public async Task HandleModMailMessage(SocketMessage message, SocketGuild guild, DiscordSocketClient client)
        {

            if (message.Author.IsBot) { return; }

            if (message.Content.ToLower() == "thanks inet") await message.Channel.SendMessageAsync("you're welcome!");

            if (message.Content.ToLower().Contains("skibidi") || message.Content.ToLower().Contains("sigma")) await ((SocketUserMessage)message).ReplyAsync("https://cdn.discordapp.com/attachments/575033344002359298/1304824028074082325/skibidi.png");

            //initialize a list of modmails and punishments for use across the class
            ticketFileRoot = ModMailTicketFileRoot.GetModMailTickets();
            PunishmentFileRoot punishments = PunishmentFileRoot.GetPunishments();

            foreach (var item in ticketFileRoot.ModMailTicketList)
            {
                if (reversedModMailTickets == null)
                {
                    reversedModMailTickets = new List<ModMailTicket>();
                }
                reversedModMailTickets.Add(item);
            }
            if (reversedModMailTickets.Count != 0)
            {
                reversedModMailTickets.Reverse();
            }

            //we use these in other methods
            sourceMessage = message;
            sourceGuild = guild;
            _client = client;
            //modmailRole = sourceGuild.GetRole(modmailRoleId);

            //for new modmails created from punishment notifications
            if (message.Reference != null && message.Channel is SocketDMChannel)
            {
                foreach (var item in reversedModMailTickets)
                {
                    if (item.isOpen && item.userID == message.Author.Id)
                    {
                        //user trying to open another ticket
                        EmbedBuilder msgEmbedBuilder = new EmbedBuilder()
                            .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                            .WithTitle($"__Ticket already open!__")
                            .WithDescription($"You already have an __open ticket__ with the ID **#{item.ticketID}**! You __cannot__ open another one.")
                            .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756404158599210/red.jpg")
                            .WithFooter("Trying to reply? Just send a message and it will get sent straight to the staff team!")
                            .WithColor(Color.Red);

                        await message.Author.SendMessageAsync(embed: msgEmbedBuilder.Build());
                        return;
                    }
                    else
                    {

                    }
                }

                foreach (var punish in punishments.punishmentList)
                {
                    if (punish.notifMsgID == message.Reference.MessageId.Value)
                    {
                        //open ticket from punishment
                        await CreateModMailFromPunishment(punish);
                        return;
                    }
                }

            }
            //for USER replies to existing open modmails
            else if (message.Reference == null && message.Channel is SocketDMChannel)
            {
                if (ticketFileRoot.ModMailTicketList.Count == 0) await CreateModMail();

                ModMailTicket? foundTicket = null;

                foreach (var item in reversedModMailTickets)
                {
                    if (item.isOpen == true && item.userID == message.Author.Id)
                    {
                        foundTicket = item;
                        break;
                    }
                }

                if (foundTicket != null)
                {
                    await AddNewUserMessage(foundTicket);
                }
                else
                {
                    await CreateModMail();
                }

                return;
            }
            else if (message.Channel is SocketThreadChannel)
            {
                foreach (var item in reversedModMailTickets)
                {
                    //i think this is the best way to find out if the source channel is a modmail channel or not
                    //it probably isnt even close to being a good way
                    if (message.Channel.Id == item.channelID)
                    {
                        if (message.Content.StartsWith("="))
                        {
                            string msg;
                            msg = message.Content.Remove(0, 1);

                            string command = msg.Split(" ")[0];

                            switch (command)
                            {
                                case "close":
                                    await CloseModMail(item, msg, false);
                                    break;
                                case "qclose":
                                    await CloseModMail(item, msg, true);
                                    break;
                                case "reopen":
                                    await ReopenModMail(item);
                                    break;
                                default:
                                    await AddNewModMessage(item);
                                    break;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    //if not a modmail channel return
                    //else { return; }
                }
            }
        }

        public async Task ReopenModMail(ModMailTicket ticket)
        {
            foreach (var item in reversedModMailTickets)
            {
                //cant reopen a ticket if the user already has an open ticket
                if (item.isOpen && item.userID == ticket.userID)
                {
                    //user trying to open another ticket
                    EmbedBuilder failEmbedBuilder = new EmbedBuilder()
                        .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                        .WithTitle($"__Ticket already open!__")
                        .WithDescription($"The user already has an __open ticket__ with the ID **#{item.ticketID}**! One user __cannot__ have multiple open tickets.")
                        .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756404158599210/red.jpg")
                        .WithFooter("Close the open ticket first.")
                        .WithColor(Color.Red);

                    await sourceMessage.Channel.SendMessageAsync(embed: failEmbedBuilder.Build());
                    return;
                }
            }

            //no open tickets, so reopen the current ticket
            ticket.isOpen = true;

            SocketUser user = _client.GetUser(ticket.userID);

            //send the message to the user
            EmbedBuilder msgEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Ticket reopened!__")
                .WithDescription($"Your ModMail ticket **#{ticket.ticketID}** has been reopened by staff!");

            await user.SendMessageAsync(embed: msgEmbedBuilder.Build());

            //send a confirmation message to the modmail channel
            EmbedBuilder confirmEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Ticket reopened!__")
                .WithDescription($"You've successfully reopened this ticket. Please send a message.")
                .WithColor(Color.Green);

            await sourceMessage.Channel.SendMessageAsync(embed: confirmEmbedBuilder.Build());

            await sourceMessage.DeleteAsync();

            await SaveModmails();
        }

        public async Task CreateModMail()
        {
            SocketThreadChannel ticketChannel;

            //create a new modmailmessage from the message that opened this ticket 
            ModMailMessage mailMessage = new()
            {
                authorID = sourceMessage.Author.Id,
                messageID = sourceMessage.Id,
                content = sourceMessage.Content
            };

            //create new ticket and populate fields
            ModMailTicket ticket = new()
            {
                ticketID = ticketFileRoot.modmailIndex,
                userID = sourceMessage.Author.Id,
                punishmentID = null,
                isOpen = true,
                associatedMessages = [mailMessage]
            };
            ticketFileRoot.modmailIndex++;

            Emoji emoji = new Emoji("✅");
            await sourceMessage.AddReactionAsync(emoji);

            EmbedBuilder replyEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceGuild.Name} [{sourceGuild.Id}]", sourceGuild.IconUrl)
                .WithTitle($"Modmail Created!")
                .WithDescription($"Your Modmail with the ID **{ticket.ticketID}** has been __successfully opened__!\nPlease be patient until a staff member gets back to you!")
                .WithColor(Color.Green)
                .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756751249576006/green.jpg")
                .WithFooter("To reply, just send a message! The ticket will be closed by staff once we deem the matter resolved.");

            try
            {
                ticketChannel = await sourceGuild.GetTextChannel(modmailChannelId).CreateThreadAsync($"{sourceMessage.Author.Username}-{ticket.ticketID}", ThreadType.PrivateThread, ThreadArchiveDuration.OneWeek, null, null, null, null);

                ticket.channelID = ticketChannel.Id;
            }
            catch (Exception)
            {
                throw;
            }

            EmbedBuilder notifEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"New Modmail!")
                .WithDescription($"A new ModMail with ID {ticket.ticketID} has been opened!")
                .WithColor(Color.Green)
                .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756751249576006/green.jpg")
                .WithFooter("To reply, send '=<message>'! To close, send '=close <reason>'");

            var roleList = string.Join(", ", sourceGuild.GetUser(sourceMessage.Author.Id).Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

            EmbedBuilder openEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"New Modmail!")
                .WithDescription($"A new ModMail with ID {ticket.ticketID} has been opened with reason `{mailMessage.content}`!")
                .AddField($":nerd: Author", $":cake: <t:{sourceMessage.Author.CreatedAt.ToUnixTimeSeconds()}:f>\n:trumpet: <t:{sourceGuild.GetUser(sourceMessage.Author.Id).JoinedAt.Value.ToUnixTimeSeconds()}:f>\n:crossed_swords: {roleList}")
                .WithColor(Color.Green)
                .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756751249576006/green.jpg")
                .WithFooter("To reply, send '=<message>'! To close, send '=close <reason>'");

            ComponentBuilder buttonBuilder = new ComponentBuilder()
                .WithButton("Jump to thread", null, ButtonStyle.Link, null, $"https://canary.discord.com/channels/{sourceGuild.Id}/{ticket.channelID}");

            //await ticketChannel.SendMessageAsync(modmailRole.Mention);
            await ticketChannel.SendMessageAsync(embed: openEmbedBuilder.Build());
            await sourceGuild.GetTextChannel(modmailChannelId).SendMessageAsync(embed: notifEmbedBuilder.Build(), components: buttonBuilder.Build());

            await sourceMessage.Author.SendMessageAsync(embed: replyEmbedBuilder.Build());

            EmbedBuilder msgEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"New message!")
                .WithDescription($"{sourceMessage.Content}");

            await ticketChannel.SendMessageAsync(embed: msgEmbedBuilder.Build());

            if (ticketFileRoot.ModMailTicketList != null)
            {
                ticketFileRoot.ModMailTicketList.Add(ticket);
            }
            else
            {
                ticketFileRoot.ModMailTicketList = [ticket];
            }

            await SaveModmails();
        }

        public async Task CloseModMail(ModMailTicket ticket, string msg, bool quiet)
        {
            if (msg.Length <= 6)
            {
                EmbedBuilder failEmbedBuilder = new EmbedBuilder()
                    .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                    .WithTitle("__No reason provided__")
                    .WithDescription(":prohibited: Please provide a reason!")
                    .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756404158599210/red.jpg")
                    .WithFooter("Closing without a reason is rude.")
                    .WithColor(Color.Red);

                await sourceMessage.Channel.SendMessageAsync(embed: failEmbedBuilder.Build());
            }


            string reason = msg.Remove(0, 6);

            ticket.isOpen = false;
            ticket.closingReason = reason;
            ticket.closingModID = sourceMessage.Author.Id;

            ModMailMessage mailMessage = new()
            {
                authorID = sourceMessage.Author.Id,
                messageID = sourceMessage.Id,
                content = reason
            };

            //associatedmessages logically cannot be null here
            ticket.associatedMessages.Add(mailMessage);

            SocketUser user = _client.GetUser(ticket.userID);

            //send the message to the user
            EmbedBuilder msgEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Ticket Closed!__")
                .WithDescription($"Your ModMail ticket has been closed with the reason `{reason}`");

            //send a confirmation message to the modmail thread
            EmbedBuilder confirmEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Ticket Closed!__")
                .WithDescription($"The ticket #{ticket.ticketID} has been closed with reason `{reason}`.")
                .WithColor(Color.Green);


            //send a confirmation message to the modmail channel
            EmbedBuilder notifEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Ticket Closed!__")
                .WithDescription($"The ticket #{ticket.ticketID} has been closed with reason `{reason}`.")
                .WithColor(Color.Red);

            if (quiet) confirmEmbedBuilder.WithDescription($"The ticket #{ticket.ticketID} has been quietly closed with reason `{reason}`.");
            if (quiet) notifEmbedBuilder.WithDescription($"The ticket #{ticket.ticketID} has been quietly closed with reason `{reason}`.");

            if (!quiet)
            {
                try
                {
                    await user.SendMessageAsync(embed: msgEmbedBuilder.Build());
                }
                catch (HttpException e)
                {
                    if (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                    {
                        confirmEmbedBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification.");
                        notifEmbedBuilder.AddField("Note!", "I couldn't send the user a DM. They will not receive the notification.");
                    }
                }
                catch (NullReferenceException e)
                {
                    if (user == null)
                    {
                        confirmEmbedBuilder.AddField("Note!", "User could not be found! They will not receive the notification.");
                        notifEmbedBuilder.AddField("Note!", "User could not be found! They will not receive the notification.");
                    }
                    else
                    {
                        confirmEmbedBuilder.AddField("Note!", $"Unknown error! {e.Message}");
                        notifEmbedBuilder.AddField($"Note!", $"Unknown error! {e.Message}");
                    }
                }
                catch (Exception e)
                {
                    confirmEmbedBuilder.AddField("Note!", $"Unknown error! {e.Message}");
                    notifEmbedBuilder.AddField($"Note!", $"Unknown error! {e.Message}");
                }
            }

            await sourceMessage.Channel.SendMessageAsync(embed: confirmEmbedBuilder.Build());


            ComponentBuilder buttonBuilder = new ComponentBuilder()
                .WithButton("Jump to thread", null, ButtonStyle.Link, null, $"https://canary.discord.com/channels/{sourceGuild.Id}/{ticket.channelID}");

            await sourceGuild.GetTextChannel(modmailChannelId).SendMessageAsync(embed: notifEmbedBuilder.Build(), components: buttonBuilder.Build());

            await sourceMessage.DeleteAsync();

            await sourceGuild.GetThreadChannel(sourceMessage.Channel.Id).ModifyAsync(x =>
            {
                x.Locked = true;
                x.Archived = true;
            });

            await SaveModmails();
        }

        public async Task AddNewModMessage(ModMailTicket ticket)
        {
            if (!ticket.isOpen)
            {
                //send a fail message to the modmail channel
                EmbedBuilder failEmbedBuilder = new EmbedBuilder()
                    .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                    .WithTitle($"__Ticket Closed__")
                    .WithDescription($"The ticket you're trying to reply to is already closed.")
                    .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756404158599210/red.jpg")
                    .WithFooter("To reopen, send '=reopen'")
                    .WithColor(Color.Red);

                await sourceMessage.Channel.SendMessageAsync(embed: failEmbedBuilder.Build());
                return;
            }

            string message = sourceMessage.Content.Remove(0, 1);

            ModMailMessage mailMessage = new()
            {
                authorID = sourceMessage.Author.Id,
                messageID = sourceMessage.Id,
                content = message
            };

            //associatedmessages logically cannot be null here
            ticket.associatedMessages.Add(mailMessage);

            SocketUser user = _client.GetUser(ticket.userID);

            //send the message to the user
            EmbedBuilder msgEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Message received__")
                .WithDescription($"{message}");

            if (sourceMessage.Attachments.Count > 0) msgEmbedBuilder.WithImageUrl(sourceMessage.Attachments.FirstOrDefault().Url);

            await user.SendMessageAsync(embed: msgEmbedBuilder.Build());

            //send a confirmation message to the modmail channel
            EmbedBuilder confirmEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Message sent__")
                .WithDescription($"{message}")
                .WithColor(Color.Green);

            if (sourceMessage.Attachments.Count > 0) confirmEmbedBuilder.WithImageUrl(sourceMessage.Attachments.FirstOrDefault().Url);


            await sourceMessage.Channel.SendMessageAsync(embed: confirmEmbedBuilder.Build());

            await sourceMessage.DeleteAsync();

            await SaveModmails();
        }

        public async Task AddNewUserMessage(ModMailTicket ticket)
        {
            SocketTextChannel ticketChannel = sourceGuild.GetTextChannel(ticket.channelID);

            ModMailMessage mailMessage = new()
            {
                authorID = sourceMessage.Author.Id,
                messageID = sourceMessage.Id,
                content = sourceMessage.Content
            };

            //associatedmessages logically cannot be null here
            ticket.associatedMessages.Add(mailMessage);

            //send this message to the modmail channel
            EmbedBuilder msgEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Message received__")
                .WithDescription($"{sourceMessage.Content}");

            if (sourceMessage.Attachments.Count > 0) msgEmbedBuilder.WithImageUrl(sourceMessage.Attachments.FirstOrDefault().Url);

            await ticketChannel.SendMessageAsync(embed: msgEmbedBuilder.Build());

            //send a confirmation to the user
            EmbedBuilder confirmEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"__Message sent__")
                .WithDescription($"{sourceMessage.Content}")
                .WithColor(Color.Green);

            await sourceMessage.Author.SendMessageAsync(embed: confirmEmbedBuilder.Build());

            Emoji emoji = new Emoji("✅");
            await sourceMessage.AddReactionAsync(emoji);

            await SaveModmails();
        }

        public async Task CreateModMailFromPunishment(Punishment punishment)
        {
            SocketThreadChannel ticketChannel;

            //get message and emote strings for punishment
            string typeText = Commands.getTypeTexts(punishment.type)[0];
            string emote = Commands.getTypeTexts(punishment.type)[1];

            //create a new modmailmessage from the message that opened this ticket 
            ModMailMessage mailMessage = new()
            {
                authorID = sourceMessage.Author.Id,
                messageID = sourceMessage.Id,
                content = sourceMessage.Content
            };

            //create new ticket and populate fields
            ModMailTicket ticket = new()
            {
                ticketID = ticketFileRoot.modmailIndex,
                userID = sourceMessage.Author.Id,
                punishmentID = punishment.punishmentID,
                isOpen = true,
                associatedMessages = [mailMessage]
            };
            ticketFileRoot.modmailIndex++;

            EmbedBuilder replyEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceGuild.Name} [{sourceGuild.Id}]", sourceGuild.IconUrl)
                .WithTitle($"Modmail Created!")
                .WithDescription($"Your Modmail with the ID **{ticket.ticketID}** for punishment **#{ticket.punishmentID}** has been __successfully opened__!\nPlease be patient until a staff member gets back to you!")
                .WithColor(Color.Green)
                .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756751249576006/green.jpg")
                .WithFooter("To reply, just send a message! The ticket will be closed by staff once we deem the matter resolved. You can always reopen it if you have further questions.");

            try
            {
                ticketChannel = await sourceGuild.GetTextChannel(modmailChannelId).CreateThreadAsync($"{sourceMessage.Author.Username}-{ticket.ticketID}", ThreadType.PublicThread, ThreadArchiveDuration.OneWeek, null, null, null, null);

                ticket.channelID = ticketChannel.Id;
            }
            catch (Exception)
            {
                throw;
            }

            await sourceMessage.Author.SendMessageAsync(embed: replyEmbedBuilder.Build());

            if (ticketFileRoot.ModMailTicketList != null)
            {
                ticketFileRoot.ModMailTicketList.Add(ticket);
            }
            else
            {
                ticketFileRoot.ModMailTicketList = [ticket];
            }

            await SaveModmails();

            EmbedBuilder notifEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"New Modmail!")
                .WithDescription($"A new ModMail with ID {ticket.ticketID} has been opened! This ticket has been created in response to a punishment.")
                .WithColor(Color.Green)
                .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756751249576006/green.jpg")
                .WithFooter("To reply, send '=<message>'! To close, send '=close <reason>'");

            string roleList = "";
            long joinedat = 0;
            if (sourceGuild.GetUser(sourceMessage.Author.Id) != null)
            {
                long joinedAt = sourceGuild.GetUser(sourceMessage.Author.Id).JoinedAt.Value.ToUnixTimeSeconds();
                roleList = string.Join(", ", sourceGuild.GetUser(sourceMessage.Author.Id).Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));
            }
            else
            {
                roleList = "User not in server!";
            }

            EmbedBuilder openEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"New Modmail!")
                .WithDescription($"A new ModMail with ID {ticket.ticketID} has been opened with reason `{mailMessage.content}`! This ticket has been created in response to a punishment:")
                .AddField($"{emote} {typeText}", $":hash: **#{ticket.ticketID}**\n:clock8: <t:{punishment.timestamp}:f>\n:dart: <@{punishment.targetID}>\n:cop: <@{punishment.modID}>\n** Reason**:\n`{punishment.reason}`", true)
                .AddField($":nerd: Author", $":cake: <t:{sourceMessage.Author.CreatedAt.ToUnixTimeSeconds()}:f>\n:trumpet: <t:{joinedat}:f>\n:crossed_swords: {roleList}", true)
                .WithColor(Color.Green)
                .WithImageUrl("https://cdn.discordapp.com/attachments/575033344002359298/1244756751249576006/green.jpg")
                .WithFooter("To reply, send '=<message>'! To close, send '=close <reason>'");

            //await ticketChannel.SendMessageAsync(modmailRole.Mention);
            await sourceGuild.GetTextChannel(modmailChannelId).SendMessageAsync(embed: notifEmbedBuilder.Build());
            await ticketChannel.SendMessageAsync(embed: openEmbedBuilder.Build());

            EmbedBuilder msgEmbedBuilder = new EmbedBuilder()
                .WithAuthor($"{sourceMessage.Author.Username} [{sourceMessage.Author.Id}]", sourceMessage.Author.GetAvatarUrl() ?? sourceMessage.Author.GetDefaultAvatarUrl())
                .WithTitle($"New message!")
                .WithDescription($"{sourceMessage.Content}");

            await ticketChannel.SendMessageAsync(embed: msgEmbedBuilder.Build());

            Emoji emoji = new Emoji("✅");
            await sourceMessage.AddReactionAsync(emoji);
        }

        public async Task SaveModmails()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\modmailtickets.json", JsonConvert.SerializeObject((object)this.ticketFileRoot, Formatting.Indented));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                File.WriteAllText("/home/vendell/inet/modmailtickets.json", JsonConvert.SerializeObject((object)this.ticketFileRoot, Formatting.Indented));
            }
        }
    }
}
