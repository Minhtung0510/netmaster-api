namespace NetMasterAPI.Models;

public class GeminiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-flash";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
    public int MaxOutputTokens { get; set; } = 1024;
    public double Temperature { get; set; } = 0.7;
}
