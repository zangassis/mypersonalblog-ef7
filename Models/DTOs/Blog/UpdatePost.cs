namespace MyPersonalBlog.Models.DTOs.Blog;
public class UpdatePost
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Text { get; set; }
    public string? AuthorName { get; set; }
    public List<UpdateTag>? Tags { get; set; }
}
