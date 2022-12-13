namespace MyPersonalBlog.Models.DTOs.Blog;
public class BlogPost
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Text { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdateDate { get; set; }
    public List<Tag> Tags { get; set; }
}