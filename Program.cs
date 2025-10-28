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
