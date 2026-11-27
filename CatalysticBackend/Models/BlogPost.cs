namespace CatalysticBackend.Models;

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ICollection<BlogLabel> Labels { get; set; } = new List<BlogLabel>();
    public DateTime ReleaseTime { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
