namespace NetMasterAPI.Models;

public class ChatResponse
{
    public bool Success { get; set; }
    public string? Reply { get; set; }
    public string? Error { get; set; }
    public string? SuggestedTopic { get; set; }
    public List<string>? Keywords { get; set; }
    public string? SuggestedExercise { get; set; }
    public long TokensUsed { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public static ChatResponse Ok(string reply, long tokens = 0, string? topic = null,
        List<string>? keywords = null, string? exercise = null) => new()
    {
        Success = true,
        Reply = reply,
        TokensUsed = tokens,
        SuggestedTopic = topic,
        Keywords = keywords,
        SuggestedExercise = exercise
    };

    public static ChatResponse Fail(string error) => new()
    {
        Success = false,
        Error = error
    };
}
