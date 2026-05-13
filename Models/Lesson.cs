namespace NetMasterAPI.Models;

public class Lesson
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Chapter { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Overview { get; set; } = string.Empty;
    public string Theory { get; set; } = string.Empty;
    public string CodeExample { get; set; } = string.Empty;
    public List<string> Exercises { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
}
