using NetMasterAPI.Models;
using NetMasterAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURATION =====
builder.Services.Configure<GeminiConfig>(options =>
{
    var apiKey = builder.Configuration["Gemini:ApiKey"]
        ?? builder.Configuration["GeminiApiKey"]
        ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? string.Empty;

    options.ApiKey = apiKey;
    options.Model = builder.Configuration["Gemini:Model"] ?? "gemini-1.5-flash";
    options.BaseUrl = builder.Configuration["Gemini:BaseUrl"]
        ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
    options.MaxOutputTokens = int.Parse(builder.Configuration["Gemini:MaxOutputTokens"] ?? "1024");
    options.Temperature = double.Parse(builder.Configuration["Gemini:Temperature"] ?? "0.7");
});

builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// ===== CONTROLLERS & API =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== CORS — cho phép frontend Vercel truy cập =====
var allowedOrigins = new List<string>
{
    "http://localhost:3000",
    "https://*.vercel.app",
    "https://*.vercel.com"
};

builder.Configuration.GetSection("AllowedOrigins").Bind(allowedOrigins);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Request-Id");
    });
});

// ===== LOGGING =====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(
    builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information);

var app = builder.Build();

// ===== PIPELINE =====
app.UseCors("AllowFrontend");

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "NetMasterAPI",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
})).WithTags("Health");

// ===== RUN =====
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
var url = $"http://0.0.0.0:{port}";

app.Logger.LogInformation("NetMasterAPI starting on {Url}", url);
app.Urls.Clear();
app.Urls.Add(url);

app.Run();
