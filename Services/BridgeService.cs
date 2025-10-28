using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

public class BridgeService
{
    public ConcurrentQueue<string> ToMinecraftQueue { get; } = new();
    private DiscordSocketClient? _discordClient;
    private readonly IConfiguration _configuration;

    public BridgeService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SetDiscordClient(DiscordSocketClient client)
    {
        _discordClient = client;
    }

    public async Task SendToDiscordAsync(string message)
    {
        if (_discordClient == null) return;
        ulong channelId = ulong.Parse(_configuration["Discord:ChannelId"]!);
        var channel = _discordClient.GetChannel(channelId) as ISocketMessageChannel;
        if (channel != null)
        {
            await channel.SendMessageAsync(message);
        }
    }
}