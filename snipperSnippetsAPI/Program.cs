using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<ApplicatioDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
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