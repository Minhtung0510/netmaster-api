using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using NetMasterAPI.Models;

namespace NetMasterAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurriculumController : ControllerBase
{
    private static readonly List<Lesson> _lessons;
    private readonly ILogger<CurriculumController> _logger;

    static CurriculumController()
    {
        var basePath = AppContext.BaseDirectory;
        var jsonPath = Path.Combine(basePath, "Data", "curriculum.json");

        if (File.Exists(jsonPath))
        {
            var json = File.ReadAllText(jsonPath);
            _lessons = JsonSerializer.Deserialize<List<Lesson>>(json) ?? new List<Lesson>();
        }
        else
        {
            _lessons = new List<Lesson>();
        }
    }

    public CurriculumController(ILogger<CurriculumController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<List<Lesson>> GetAll()
    {
        return Ok(_lessons);
    }

    [HttpGet("{id}")]
    public ActionResult<Lesson> GetById(string id)
    {
        var lesson = _lessons.FirstOrDefault(l => l.Id == id);
        if (lesson == null)
        {
            _logger.LogWarning("Lesson not found: {Id}", id);
            return NotFound(new { error = $"Bài học '{id}' không tìm thấy." });
        }
        return Ok(lesson);
    }
}
