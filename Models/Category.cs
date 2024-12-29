namespace Skyfall.Models;

public class Category
{
    public required string Name { get; set; }
    public List<string> Words { get; set; } = [];
}
