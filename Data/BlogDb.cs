using Microsoft.EntityFrameworkCore;
using MyPersonalBlog.Models.DTOs.Blog;

namespace MyPersonalBlog.Data;

public class BlogDb : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
         options.UseSqlite("DataSource = blog; Cache=Shared");

    public DbSet<BlogPost> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }
}