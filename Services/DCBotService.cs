using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// Renamed class from DiscordBotService
public class DCBotService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly BridgeService _bridge;
    private readonly DiscordSocketClient _client;

    // Renamed constructor
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

        string formattedMessage = $"[Discord] {message.Author.Username}: {message.Content}";
        _bridge.ToMinecraftQueue.Enqueue(formattedMessage);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }
}