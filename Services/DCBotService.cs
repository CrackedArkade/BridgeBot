using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

<<<<<<< HEAD
<<<<<<< HEAD

=======
// Renamed class from DiscordBotService
>>>>>>> 4a248c5 (New Controllers and Services Added)
=======

>>>>>>> 68f5609 (Updates to the code (Readded !ping functionality and other changes))
public class DCBotService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly BridgeService _bridge;
    private readonly DiscordSocketClient _client;

<<<<<<< HEAD
<<<<<<< HEAD

=======
    // Renamed constructor
>>>>>>> 4a248c5 (New Controllers and Services Added)
=======

>>>>>>> 68f5609 (Updates to the code (Readded !ping functionality and other changes))
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

<<<<<<< HEAD
<<<<<<< HEAD
private async Task OnMessageReceived(SocketMessage message)
{

    if (message.Author.IsBot) return;


    ulong targetChannelId = ulong.Parse(_configuration["Discord:ChannelId"]!);
    

   if (message.Channel.Id != targetChannelId) return;


    if (message.Content == "!ping")
    {

        await message.Channel.SendMessageAsync("Pong!");
        

        return;
    }

    string dcToMc = $"[Discord] {message.Author.Username}: {message.Content}";
    
    _bridge.ToMinecraftQueue.Enqueue(dcToMc);
}

=======
    private Task OnMessageReceived(SocketMessage message)
=======
private async Task OnMessageReceived(SocketMessage message)
{

    if (message.Author.IsBot) return;


    ulong targetChannelId = ulong.Parse(_configuration["Discord:ChannelId"]!);
    

   if (message.Channel.Id != targetChannelId) return;


    if (message.Content == "!ping")
>>>>>>> 68f5609 (Updates to the code (Readded !ping functionality and other changes))
    {

        await message.Channel.SendMessageAsync("Pong!");
        

        return;
    }

    string dcToMc = $"[Discord] {message.Author.Username}: {message.Content}";
    
    _bridge.ToMinecraftQueue.Enqueue(dcToMc);
}

>>>>>>> 4a248c5 (New Controllers and Services Added)
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }
}