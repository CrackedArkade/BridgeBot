using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/minecraft-chat")]
// Renamed class from MinecraftChatController
public class MCChatController : ControllerBase
{
    private readonly BridgeService _bridge;

    // Renamed constructor
    public MCChatController(BridgeService bridge)
    {
        _bridge = bridge;
    }

    [HttpPost]
    public async Task<IActionResult> PostFromMinecraft([FromBody] MinecraftMessage payload)
    {
        if (payload == null || string.IsNullOrEmpty(payload.Content))
        {
            return BadRequest("Invalid payload.");
        }

        string formattedMessage = $"**{payload.PlayerName}**: {payload.Content}";
        await _bridge.SendToDiscordAsync(formattedMessage);
        
        return Ok();
    }

    [HttpGet]
    public IActionResult GetToMinecraft()
    {
        var messages = new List<string>();
        while (_bridge.ToMinecraftQueue.TryDequeue(out var message))
        {
            messages.Add(message);
        }
        return Ok(messages);
    }
}

// This class can stay here as it's only used by this controller.
public class MinecraftMessage
{
    public string PlayerName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}