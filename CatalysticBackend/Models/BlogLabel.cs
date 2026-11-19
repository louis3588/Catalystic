namespace CatalysticBackend.Models;

public class BlogLabel
{
    public int Id { get; set; }
    public int BlogId { get; set; }
    public BlogPost Blog { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
}