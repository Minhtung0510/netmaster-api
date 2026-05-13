using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using NetMasterAPI.Models;

namespace NetMasterAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurriculumController : ControllerBase
{
    private static readonly List<Lesson> _lessons;

    static CurriculumController()
    {
        var basePath = AppContext.BaseDirectory;
        var jsonPath = Path.Combine(basePath, "Data", "curriculum.json");

        if (System.IO.File.Exists(jsonPath))
        {
            var json = System.IO.File.ReadAllText(jsonPath);
            _lessons = JsonSerializer.Deserialize<List<Lesson>>(json) ?? new List<Lesson>();
        }
        else
        {
            _lessons = new List<Lesson>();
        }
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
            return NotFound(new { error = $"Bai hoc '{id}' khong tim thay." });
        }
        return Ok(lesson);
    }
}
