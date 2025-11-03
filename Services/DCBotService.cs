using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;


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


        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;


        _client.MessageReceived += OnMessageReceived;

        await _client.LoginAsync(TokenType.Bot, _configuration["Discord:Token"]);
        await _client.StartAsync();

        _bridge.SetDiscordClient(_client);
    }

     public async Task Client_Ready()
    {
        var playSoundCommand = new SlashCommandBuilder()
            .WithName("playsound")
            .WithDescription("Plays a sound effect for a player in Minecraft.")
            .AddOption("player", ApplicationCommandOptionType.String, "The in-game name of the player.", isRequired: true);

        var soundOption = new SlashCommandOptionBuilder()
            .WithName("sound")
            .WithDescription("The sound effect to play.")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String)
            .AddChoice("Creeper Hiss", "ENTITY_CREEPER_PRIMED")
            .AddChoice("Zombie Groan", "ENTITY_ZOMBIE_AMBIENT")
            .AddChoice("Level Up", "ENTITY_PLAYER_LEVELUP")
            .AddChoice("Ghast Scream", "ENTITY_GHAST_SCREAM")
            .AddChoice("Villager No", "ENTITY_VILLAGER_NO")
            .AddChoice("Anvil Land", "BLOCK_ANVIL_LAND");

        playSoundCommand.AddOption(soundOption);

        try
        {
            await _client.CreateGlobalApplicationCommandAsync(playSoundCommand.Build());
            Console.WriteLine("Successfully registered the /playsound command.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering command: {ex.Message}");
        }
    }
    private async Task SlashCommandHandler(SocketSlashCommand command)

    {
        if (command.Data.Name == "playsound")

        {
            string playerName = (string)command.Data.Options.First(o => o.Name == "player").Value;
            string soundName = (string)command.Data.Options.First(o => o.Name == "sound").Value;
            var soundRequest = new SoundRequest
            {
                PlayerName = playerName,
                SoundName = soundName
            };
            _bridge.ToMinecraftSoundQueue.Enqueue(soundRequest);
            await command.RespondAsync($"Sent the '{soundName}' sound to '{playerName}'!", ephemeral: true);
        }
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