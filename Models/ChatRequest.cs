using System.ComponentModel.DataAnnotations;

namespace NetMasterAPI.Models;

public class ChatRequest
{
    [Required]
    [MinLength(1)]
    public List<ChatMessage> Messages { get; set; } = new();

    public string? CurrentTopic { get; set; }

    public string? Language { get; set; } = "vi";
}

public class ChatMessage
{
    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}
