using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


public class DCBotService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly BridgeService _bridge;
    private readonly DiscordSocketClient _client;


    public DCBotService(IConfiguration configuration, BridgeService bridge)
    {
        _configuration = configuration;
        _bridge = bridge;

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
        };
        _client = new DiscordSocketClient(config);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += (msg) => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };
        _client.Ready += () => { Console.WriteLine("Discord Bot is connected!"); return Task.CompletedTask; };
        _client.MessageReceived += OnMessageReceived;

        await _client.LoginAsync(TokenType.Bot, _configuration["Discord:Token"]);
        await _client.StartAsync();

        _bridge.SetDiscordClient(_client);
    }

    private Task OnMessageReceived(SocketMessage message)
    {

        if (message.Author.IsBot) return Task.CompletedTask;

        ulong targetChannelId = ulong.Parse(_configuration["Discord:ChannelId"]!);


        if (message.Channel.Id != targetChannelId) return Task.CompletedTask;


        if (message.Content == "!ping")
        {

            message.Channel.SendMessageAsync("Pong!");


            return Task.CompletedTask;
        }

        string userHexColor = "#FFFFFF";
        string authorName = message.Author.Username;
        if (message.Author is SocketGuildUser guildUser)
        {
            authorName = guildUser.DisplayName;

            var userColor = guildUser.Roles
                .Where(role => role.Color.RawValue != 0)
                .OrderByDescending(role => role.Position)
                .FirstOrDefault()?.Color;

            if (userColor.HasValue)
            {
                userHexColor = $"#{userColor.Value.R:X2}{userColor.Value.G:X2}{userColor.Value.B:X2}";
            }
        }

        var messageInfo = new DiscordMessageInfo
        {
            Author = authorName,
            Content = message.Content,
            HexColor = userHexColor
        };

        _bridge.ToMinecraftQueue.Enqueue(messageInfo);
        return Task.CompletedTask;
    
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }
}