namespace MyPersonalBlog.Models.DTOs.Blog;
public class CreatePost
{
    public string? Title { get; set; }
    public string? Text { get; set; }
    public string? AuthorName { get; set; }
    public List<CreateTag>? Tags { get; set; }
}
