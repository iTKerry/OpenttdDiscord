﻿using Discord;
using Discord.WebSocket;
using Discord.Commands;
using OpenttdDiscord.Configuration;
using OpenttdDiscord.Openttd;
using Microsoft.Extensions.DependencyInjection;
using OpenttdDiscord.Openttd.Udp;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using OpenttdDiscord.Backend.Servers;
using OpenttdDiscord.Commands;
using System.Timers;
using Discord.Rest;

namespace OpenttdDiscord
{
    class Program
    {
        static DiscordSocketClient client;
        static ISubscribedServerService subscribedServerService;
        static IUdpOttdClient udpOttdClient;

        static Timer updateTimer = new Timer(3_000);

        public static async Task Main()
        {
            using (var services = DependencyConfig.ServiceProvider)
            {
                subscribedServerService = services.GetRequiredService<ISubscribedServerService>();
                client = services.GetRequiredService<DiscordSocketClient>();
                udpOttdClient = services.GetRequiredService<IUdpOttdClient>();
                client.Log += Log;
                services.GetRequiredService<CommandService>().Log += Log;

                updateTimer.AutoReset = true;
                updateTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);


                await client.LoginAsync(TokenType.Bot, services.GetRequiredService<OpenttdDiscordConfig>().Token);
                await client.StartAsync();

                updateTimer.Start();

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await Task.Delay(-1);
            }
        }

        private static async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            updateTimer.Stop();
            try
            {

                var servers = await subscribedServerService.GetAllServers();

                foreach (var s in servers)
                {
                    var channel = client.GetChannel(s.ChannelId) as SocketTextChannel;
                    ulong? messageId = s.MessageId;
                    if (messageId.HasValue == false || (await channel.GetMessageAsync(messageId.Value)) == null)
                    {
                        messageId = (await channel.SendMessageAsync("Getting server info")).Id;
                    }
                    if (messageId.HasValue)
                    {
                        var msg = await udpOttdClient.SendMessage(new PacketUdpClientFindServer(), s.Server.ServerIp, s.Server.ServerPort);

                        if (msg is PacketUdpServerResponse r)
                        {
                            var embed = new EmbedBuilder
                            {
                                Title = $"{r.ServerName}"
                            };

                            embed.AddField("Players", $"{r.ClientsOn}/{r.ClientsMax}", true);
                            embed.AddField("Map Size", $"{r.MapWidth}x{r.MapHeight}", true);
                            embed.AddField("Year", $"{r.GameDate.ToString()}", true);

                            embed.AddField("Climate", "Temperate", true);
                            embed.AddField("Map name", r.MapName, true);
                            embed.AddField("Language", "Polish", true);

                            embed.AddField("Server address", $"{s.Server.ServerIp}:{s.Server.ServerPort}", true);
                            embed.AddField("Password?", r.HasPassword ? "No" : "Yes", true);

                            embed.WithCurrentTimestamp();


                            await (await channel.GetMessageAsync(messageId.Value) as RestUserMessage).ModifyAsync(x =>
                            {
                                x.Embed = embed.Build();
                                x.Content = string.Empty;
                            });
                        }

                        await subscribedServerService.UpdateServer(s.Server.Id, messageId.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception " + ex.ToString());
            }

            updateTimer.Start();
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.Message);
            return Task.CompletedTask;
        }

    }
}
