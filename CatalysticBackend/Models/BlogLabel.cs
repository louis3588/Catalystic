namespace CatalysticBackend.Models;

public class BlogLabel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
}
