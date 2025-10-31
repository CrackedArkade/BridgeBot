using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;


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
        var soundChoices = new List<ApplicationCommandOptionChoiceProperties>
        {
            new ApplicationCommandOptionChoiceProperties() { Name = "Creeper Hiss", Value = "ENTITY_CREEPER_PRIMED" },
            new ApplicationCommandOptionChoiceProperties() { Name = "Zombie Groan", Value = "ENTITY_ZOMBIE_AMBIENT" },
            new ApplicationCommandOptionChoiceProperties() { Name = "Level Up", Value = "ENTITY_PLAYER_LEVELUP" },
            new ApplicationCommandOptionChoiceProperties() { Name = "Ghast Scream", Value = "ENTITY_GHAST_SCREAM" },
            new ApplicationCommandOptionChoiceProperties() { Name = "Villager No", Value = "ENTITY_VILLAGER_NO" },
            new ApplicationCommandOptionChoiceProperties() { Name = "Anvil Land", Value = "BLOCK_ANVIL_LAND" }
        };

        var playSoundCommand = new SlashCommandBuilder()
            .WithName("playsound")
            .WithDescription("Play a Minecraft sound effect.")
            .AddOption("player", ApplicationCommandOptionType.String, "The player to hear the sound.", isRequired: true)
            .AddOption("sound", ApplicationCommandOptionType.String, "The sound effect to play.", isRequired: true, choices: soundChoices);

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