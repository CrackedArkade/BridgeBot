using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;


public class DiscordMessageInfo
{
    public string Author { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string HexColor { get; set; } = "#FFFFFF";
}
public class BridgeService
{
    public ConcurrentQueue<DiscordMessageInfo> ToMinecraftQueue { get; } = new();
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