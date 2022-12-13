using Microsoft.EntityFrameworkCore;
using MyPersonalBlog.Data;
using MyPersonalBlog.Models.DTOs.Auhor;
using MyPersonalBlog.Models.DTOs.Blog;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<BlogDb>();
builder.Services.AddScoped<AuthorDb>();
var authorConnection = @"Server=(localdb)\mssqllocaldb;Database=authorDb;Trusted_Connection=True;";
builder.Services.AddDbContext<AuthorDb>(options => options.UseSqlServer(authorConnection));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region SQLITE Context endpoints

app.MapGet("/blog/getAllPosts", (BlogDb db) =>
{
    var posts = db.Posts.ToList();

    if (!posts.Any())
        return Results.NotFound();

    foreach (var post in posts)
        post.Tags = db.Tags.Where(t => t.BlogPostId == post.Id).ToList();

    return Results.Ok(posts);
})
.WithName("GetAllPosts")
.WithOpenApi();

app.MapGet("/blog/getPostById/{id}", async (Guid id, BlogDb db) =>
{
    var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
    post.Tags = db.Tags.Where(t => t.BlogPostId == id).ToList();

    if (post is null)
        return Results.NotFound($"No post found for id: {id}");

    return Results.Ok(post);
})
.WithName("GetPostById")
.WithOpenApi();

app.MapGet("/blog/getPostsGroupedByAuthor", (BlogDb db) =>
{
    var groupByAuthor = db.Posts.GroupBy(p => p.AuthorName);

    if (!groupByAuthor.Any())
        return Results.NotFound();

    return Results.Ok(groupByAuthor);
})
.WithName("GetPostsGroupedByAuthor")
.WithOpenApi();

app.MapPost("/blog/create", async (CreatePost newPost, BlogDb db) =>
{
    var tagsEntiy = new List<Tag>();
    Guid postId = Guid.NewGuid();

    foreach (var tag in newPost?.Tags)
        tagsEntiy.Add(new Tag() { Id = Guid.NewGuid(), Name = tag.Name });

    var entity = new BlogPost()
    {
        Id = postId,
        Title = newPost.Title,
        Text = newPost.Text,
        AuthorName = newPost.AuthorName,
        CreationDate = DateTime.Now,
        LastUpdateDate = DateTime.Now,
        Tags = tagsEntiy
    };

    db.Posts.Add(entity);

    await db.SaveChangesAsync();

    return TypedResults.Created($"{entity.Id}", entity);
})
.WithName("CreatePost")
.WithOpenApi();

app.MapPut("/blog/update", async (UpdatePost updatePost, BlogDb db) =>
{
    var postExists = await db.Posts.FirstOrDefaultAsync(p => p.Id == updatePost.Id);

    if (postExists is null)
        return Results.NotFound($"No post found for id: {updatePost.Id}");

    await db.Posts
    .Where(p => p.Id == updatePost.Id)
    .ExecuteUpdateAsync(s => s
    .SetProperty(b => b.AuthorName, updatePost.AuthorName)
    .SetProperty(b => b.Title, b => updatePost.Title)
    .SetProperty(b => b.Text, b => updatePost.Text)
    .SetProperty(b => b.LastUpdateDate, DateTime.Now));

    foreach (var tag in updatePost.Tags)
        await db.Tags.Where(t => t.Id == tag.TagId).ExecuteUpdateAsync(s => s.SetProperty(t => t.Name, tag.TagName));

    return TypedResults.Ok(updatePost);
})
.WithName("UpdatePost")
.WithOpenApi();

app.MapDelete("/blog/delete/{id}", async (Guid id, BlogDb db) =>
{
    var postExists = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);

    if (postExists is null)
        return Results.NotFound($"No post found for id: {id}");

    await db.Posts.Where(p => p.Id == id).ExecuteDeleteAsync();
    await db.Tags.Where(t => t.BlogPostId == id).ExecuteDeleteAsync();

    return Results.Ok("Post deleted successfully");
})
.WithName("DeletePost")
.WithOpenApi();

#endregion

#region SQL Server Context endpoints

app.MapPost("/auhtor/create", async (Author newAuthor, AuthorDb db) =>
{
    var entity = new Author()
    {
        Id = newAuthor.Id,
        Name = newAuthor.Name,
        Contact = newAuthor.Contact,
    };

    db.Authors.Add(entity);

    await db.SaveChangesAsync();

    return TypedResults.Created($"{entity.Id}", entity);
})
.WithName("CreateAuthor")
.WithOpenApi();

app.MapGet("/auhtor/getAllAuhtors", (AuthorDb db) =>
{
    var authors = db.Authors.ToList();

    if (!authors.Any())
        return Results.NotFound();

    return Results.Ok(authors);
})
.WithName("GetAllAuhtors")
.WithOpenApi();

app.MapGet("/auhtor/getAuhtorsByCity/{city}", async (AuthorDb db, string city) =>
{
    var authorsByCity = await db.Authors.Where(author => author.Contact.Address.City == city).ToListAsync();

    if (!authorsByCity.Any())
        return Results.NotFound();

    return Results.Ok(authorsByCity);
})
.WithName("GetAuhtorsByCity")
.WithOpenApi();

app.MapPut("/auhtor/update/{id}", async (AuthorDb db, Author author, Guid id) =>
{
    var authorExists = await db.Authors.Where(author => author.Id == id).FirstOrDefaultAsync();

    if (authorExists is null)
        return Results.NotFound();

    authorExists.Contact = new() { Address = new(author.Contact.Address.Street, author.Contact.Address.City, author.Contact.Address.Postcode, author.Contact.Address.Country), Phone = author.Contact.Phone };

    await db.SaveChangesAsync();

    return Results.Ok(author);
})
.WithName("UpdateAuthor")
.WithOpenApi();

app.MapGet("/blog/getAuthorsGroupedByCountry", (AuthorDb db) =>
{
    var groupByCountry = db.Authors.GroupBy(a => a.Contact.Address.Country);

    if (!groupByCountry.Any())
        return Results.NotFound();

    return Results.Ok(groupByCountry);
})
.WithName("GetAuthorsGroupedByCountry")
.WithOpenApi();

#endregion

app.Run();