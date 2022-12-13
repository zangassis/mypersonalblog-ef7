using Microsoft.EntityFrameworkCore;
using MyPersonalBlog.Models.DTOs.Auhor;

namespace MyPersonalBlog.Data;
public class AuthorDb : DbContext
{
    public AuthorDb(DbContextOptions<AuthorDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>().OwnsOne(
            author => author.Contact, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
                ownedNavigationBuilder.OwnsOne(contactDetails => contactDetails.Address);
            });
    }

    public DbSet<Author> Authors { get; set; }
}

