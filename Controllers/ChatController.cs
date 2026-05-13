using Microsoft.AspNetCore.Mvc;
using NetMasterAPI.Models;
using NetMasterAPI.Services;

namespace NetMasterAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IGeminiService geminiService, ILogger<ChatController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendMessage(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid chat request received");
            return BadRequest(ChatResponse.Fail("Yêu cầu không hợp lệ. Vui lòng kiểm tra lại dữ liệu."));
        }

        if (request.Messages == null || request.Messages.Count == 0)
        {
            return BadRequest(ChatResponse.Fail("Danh sách tin nhắn trống."));
        }

        _logger.LogInformation(
            "Chat request: {Count} messages, topic: {Topic}",
            request.Messages.Count,
            request.CurrentTopic ?? "general");

        var response = await _geminiService.SendMessageAsync(
            request.Messages,
            request.CurrentTopic,
            request.Language,
            cancellationToken);

        if (!response.Success)
        {
            _logger.LogError("Gemini service failed: {Error}", response.Error);
            return StatusCode(500, response);
        }

        return Ok(response);
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "NetMasterAPI",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
