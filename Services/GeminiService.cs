using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NetMasterAPI.Models;

namespace NetMasterAPI.Services;

public interface IGeminiService
{
    Task<ChatResponse> SendMessageAsync(List<ChatMessage> messages, string? currentTopic, string? language = "vi",
        CancellationToken cancellationToken = default);
}

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiConfig _config;
    private readonly ILogger<GeminiService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private const string SystemPrompt = @"Bạn là Mr.NET — giáo viên backend C# chuyên nghiệp với 10+ năm kinh nghiệm.

Kiến thức chuyên sâu:
- C#: OOP, Generics, LINQ, Async/Await, Delegates, Events, Records, Pattern Matching
- ASP.NET Core: Web API, Minimal APIs, Middleware, Filters, Dependency Injection
- Auth: JWT, OAuth2, Identity Framework, SSO, MFA
- EF Core: DbContext, Migrations, Relationships, Query Optimization, Raw SQL
- Architecture: Clean Architecture, CQRS, MediatR, Repository Pattern, Unit of Work
- Background Jobs: Hangfire, IHostedService, Quartz.NET
- Caching: Redis, In-Memory Cache, Response Caching
- Testing: xUnit, Moq, FluentAssertions, Integration Tests
- Docker + Linux deployment, CI/CD với GitHub Actions
- Mobile Backend: FCM Push Notifications, File Upload (S3/Blob), SignalR Real-time
- Game Backend: SignalR multiplayer, Leaderboard (Redis Sorted Sets), Matchmaking, ELO Rating

Phong cách dạy của bạn:
1. Trả lời TIẾNG VIỆT, rõ ràng, thân thiện như đang trò chuyện
2. Luôn kèm code C# hoàn chỉnh khi giải thích concept
3. Dùng ví dụ game (Unity, inventory system, boss fight, loot box) khi phù hợp
4. Sau mỗi giải thích: gợi ý 1 bài tập nhỏ để thực hành
5. Luôn suggest keywords và topic liên quan
6. Giải thích 'TẠI SAO cần' trước rồi mới 'CÁCH LÀM'
7. Nếu có code lỗi: debug chi tiết, chỉ ra nguyên nhân và fix
8. Dùng bảng, sơ đồ ASCII khi cần để trình bày kiến trúc

Format response:
- Đoạn giải thích ngắn gọn (2-4 câu)
- Code C# (nếu có) trong khối ```csharp
- Bài tập gợi ý
- Keywords và topics liên quan

Khi được hỏi về chủ đề ngoài phạm vi C#/ASP.NET Core, hãy lịch sự redirect về chủ đề chính.";

    public GeminiService(HttpClient httpClient, IOptions<GeminiConfig> config, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<ChatResponse> SendMessageAsync(List<ChatMessage> messages, string? currentTopic, string? language = "vi",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            _logger.LogError("Gemini API key is not configured");
            return ChatResponse.Fail("API key chưa được cấu hình. Vui lòng liên hệ quản trị viên.");
        }

        try
        {
            var request = BuildRequest(messages, currentTopic);
            var json = JsonSerializer.Serialize(request, JsonOptions);

            var url = $"{_config.BaseUrl}?key={_config.ApiKey}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            _logger.LogDebug("Sending request to Gemini API. Messages count: {Count}", messages.Count);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API error: {StatusCode} - {Body}", response.StatusCode, errorBody);
                return ChatResponse.Fail($"Gemini API lỗi: {response.StatusCode}. Vui lòng thử lại sau.");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson, JsonOptions);

            var reply = ExtractReply(geminiResponse);
            if (string.IsNullOrWhiteSpace(reply))
            {
                _logger.LogWarning("Empty response from Gemini API");
                return ChatResponse.Fail("Không nhận được phản hồi từ AI. Vui lòng thử lại.");
            }

            var tokens = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Length ?? 0;

            _logger.LogInformation("Gemini response received. Tokens: ~{Tokens}", tokens);
            return ChatResponse.Ok(reply, tokens);

        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Gemini request timed out");
            return ChatResponse.Fail("Yêu cầu bị hủy do timeout. Vui lòng thử lại.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling Gemini API");
            return ChatResponse.Fail("Lỗi kết nối mạng. Vui lòng kiểm tra kết nối internet.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Gemini API");
            return ChatResponse.Fail($"Đã xảy ra lỗi: {ex.Message}");
        }
    }

    private GeminiRequest BuildRequest(List<ChatMessage> messages, string? currentTopic)
    {
        var systemInstruction = SystemPrompt;
        if (!string.IsNullOrWhiteSpace(currentTopic))
        {
            systemInstruction += $"\n\n[Học sinh đang học bài: {currentTopic}]";
        }

        var contents = messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Content))
            .Select(m => new GeminiContent
            {
                Role = m.Role == "assistant" ? "model" : m.Role,
                Parts = new List<GeminiPart> { new() { Text = m.Content } }
            })
            .ToList();

        return new GeminiRequest
        {
            SystemInstruction = new GeminiSystemInstruction
            {
                Parts = new List<GeminiPart> { new() { Text = systemInstruction } }
            },
            Contents = contents,
            GenerationConfig = new GeminiGenerationConfig
            {
                MaxOutputTokens = _config.MaxOutputTokens,
                Temperature = _config.Temperature,
                TopP = 0.95,
                TopK = 40
            }
        };
    }

    private static string? ExtractReply(GeminiResponse? response)
    {
        if (response?.Candidates == null || response.Candidates.Count == 0)
        {
            if (response?.PromptFeedback?.BlockReason != null)
            {
                return $"Nội dung bị chặn bởi bộ lọc an toàn. Lý do: {response.PromptFeedback.BlockReason}";
            }
            return null;
        }

        var candidate = response.Candidates[0];

        if (candidate.FinishReason == "SAFETY")
        {
            return "Nội dung phản hồi bị chặn bởi bộ lọc an toàn. Vui lòng đặt câu hỏi khác.";
        }

        return candidate.Content?.Parts?.FirstOrDefault()?.Text?.Trim();
    }
}
