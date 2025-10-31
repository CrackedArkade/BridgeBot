using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[ApiController]
[Route("api/mc-events")]
public class MCEventController : ControllerBase
{
    private readonly BridgeService _bridge;

    public MCEventController(BridgeService bridge)
    {
        _bridge = bridge;
    }

    [HttpGet("sounds")]
    public IActionResult GetSoundEvents()
    {
        var soundsToPlay = new List<SoundRequest>();
        while (_bridge.ToMinecraftSoundQueue.TryDequeue(out var soundRequest))
        {
            soundsToPlay.Add(soundRequest);
        }
        return Ok(soundsToPlay);
    }
}

public class SoundRequest
{
    public string PlayerName  { get; set; } = string.Empty;
    public string SoundName { get; set; } = string.Empty;

}