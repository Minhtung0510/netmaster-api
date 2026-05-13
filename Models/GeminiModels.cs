namespace NetMasterAPI.Models;

public class GeminiRequest
{
    public List<GeminiContent>? Contents { get; set; }
    public GeminiSystemInstruction? SystemInstruction { get; set; }
    public GeminiGenerationConfig? GenerationConfig { get; set; }
}

public class GeminiContent
{
    public string Role { get; set; } = string.Empty;
    public List<GeminiPart> Parts { get; set; } = new();
}

public class GeminiPart
{
    public string? Text { get; set; }
}

public class GeminiSystemInstruction
{
    public List<GeminiPart> Parts { get; set; } = new();
}

public class GeminiGenerationConfig
{
    public int MaxOutputTokens { get; set; } = 1024;
    public double Temperature { get; set; } = 0.7;
    public double TopP { get; set; } = 0.95;
    public int TopK { get; set; } = 40;
}

public class GeminiResponse
{
    public List<GeminiCandidate>? Candidates { get; set; }
    public GeminiPromptFeedback? PromptFeedback { get; set; }
}

public class GeminiCandidate
{
    public GeminiContent? Content { get; set; }
    public string? FinishReason { get; set; }
    public int Index { get; set; }
    public GeminiSafetyRating[]? SafetyRatings { get; set; }
}

public class GeminiPromptFeedback
{
    public GeminiSafetyRating[]? SafetyRatings { get; set; }
    public string? BlockReason { get; set; }
}

public class GeminiSafetyRating
{
    public string? Category { get; set; }
    public string? Probability { get; set; }
}
