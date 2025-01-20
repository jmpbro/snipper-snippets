using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SnipperSnippet.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

var snippets = new List<Snippet>();

app.MapGet("/snippet", () => snippets);

app.MapGet("/snippet/{id}", Results<Ok<Snippet>, NotFound> (int id) =>
{
    var targetSnippet = snippets.SingleOrDefault(t => id == t.Id);
    return targetSnippet is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(targetSnippet);
});

app.MapPost("/snippet", (Snippet snippet) =>
{
    snippets.Add(snippet);
    return TypedResults.Created("/snippet/{snippet.Id}", snippet);
});

app.MapDelete("/snippet/{id}", (int id) =>
{
    snippets.RemoveAll(t => id == t.Id);
    return TypedResults.NoContent();
});

app.Run();

public record Snippet(int Id, string Language, string Code);