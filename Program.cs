using Microsoft.AspNetCore.Builder;
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