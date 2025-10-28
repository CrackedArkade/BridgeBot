<<<<<<< HEAD
<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddControllers();

builder.Services.AddSingleton<BridgeService>();


builder.Services.AddHostedService<DCBotService>();


var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
=======
﻿// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
>>>>>>> 8a62d22 (Initial commit: New .NET project with gitignore)
=======
﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

public class Program
{
    // FIX: Make the fields nullable with a '?' because they are not set in the constructor.
    private DiscordSocketClient? _client;
    private IConfiguration? _configuration;

    public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        // FIX: The compiler warns that the return value might be null.
        // string.IsNullOrEmpty already handles this perfectly, so no change is needed here.
        string? discordToken = _configuration["Discord:Token"];
        if (string.IsNullOrEmpty(discordToken))
        {
            LogFatal("Discord:Token is missing. Set it with 'dotnet user-secrets set \"Discord:Token\" \"your_token\"'");
            return;
        }
        
        // FIX: The compiler warns that this value could be null.
        string? channelIdString = _configuration["Discord:ChannelId"];
        if (string.IsNullOrEmpty(channelIdString) || channelIdString == "0")
        {
            LogFatal("Discord:ChannelId is not set in appsettings.json. Please set it to your target channel's ID.");
            return;
        }

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
        };

        _client = new DiscordSocketClient(config);

        _client.Log += Log;
        _client.MessageReceived += MessageReceivedAsync;
        _client.Ready += OnReady;

        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task OnReady()
    {
        Console.WriteLine("Bot is connected and ready!");
        Console.WriteLine($"Monitoring Discord Channel ID: {_configuration?["Discord:ChannelId"]}");
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        // The _client or _configuration could theoretically be null if something goes wrong during startup.
        if (_client == null || _configuration == null || message.Author.IsBot) return;

        // FIX: Use the null-forgiving operator '!' to tell the compiler we are sure this value
        // is not null because we checked for it at startup.
        ulong targetChannelId = ulong.Parse(_configuration["Discord:ChannelId"]!);
        if (message.Channel.Id != targetChannelId)
        {
            return;
        }

        if (message.Content == "!ping")
        {
            await message.Channel.SendMessageAsync("Pong!");
        }
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private void LogFatal(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"FATAL ERROR: {message}");
        Console.ResetColor();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
>>>>>>> 1ae2f63 (Initial Setup for the project)
