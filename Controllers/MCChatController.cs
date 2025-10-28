using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/minecraft-chat")]
<<<<<<< HEAD
<<<<<<< HEAD

=======
// Renamed class from MinecraftChatController
>>>>>>> 4a248c5 (New Controllers and Services Added)
=======

>>>>>>> 68f5609 (Updates to the code (Readded !ping functionality and other changes))
public class MCChatController : ControllerBase
{
    private readonly BridgeService _bridge;

<<<<<<< HEAD
<<<<<<< HEAD

=======
    // Renamed constructor
>>>>>>> 4a248c5 (New Controllers and Services Added)
=======

>>>>>>> 68f5609 (Updates to the code (Readded !ping functionality and other changes))
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

<<<<<<< HEAD
<<<<<<< HEAD

=======
// This class can stay here as it's only used by this controller.
>>>>>>> 4a248c5 (New Controllers and Services Added)
=======

>>>>>>> 68f5609 (Updates to the code (Readded !ping functionality and other changes))
public class MinecraftMessage
{
    public string PlayerName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}