using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core and Identity
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("JWT Key is missing in configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();
app.UseCors(policy =>
    policy.WithOrigins("https://your-frontend-domain.com") // Replace with allowed origin(s)
          .AllowAnyHeader()
          .AllowAnyMethod());
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();

// In-memory snippet storage
var snippets = new List<Snippet>();

// Snippet routes (secured with [Authorize])
app.MapGet("/snippet", [Authorize] () => snippets);

app.MapGet("/snippet/{id}", [Authorize] (int id) =>
{
    var targetSnippet = snippets.SingleOrDefault(t => t.Id == id);
    return targetSnippet is null
        ? Results.NotFound()
        : Results.Ok(targetSnippet);
});

app.MapPost("/snippet", [Authorize] (Snippet snippet) =>
{
    if (string.IsNullOrWhiteSpace(snippet.Code) || string.IsNullOrWhiteSpace(snippet.Language))
        return Results.BadRequest("Snippet Code and Language cannot be empty.");

    snippets.Add(snippet);
    return Results.Created($"/snippet/{snippet.Id}", snippet);
});

app.MapDelete("/snippet/{id}", [Authorize] (int id) =>
{
    var deletedCount = snippets.RemoveAll(t => t.Id == id);
    return deletedCount > 0
        ? Results.NoContent()
        : Results.NotFound();
});

// Authentication routes
app.MapPost("/api/auth/register", async (UserManager<IdentityUser> userManager, RegisterModel model) =>
{
    if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        return Results.BadRequest("Username, Email, and Password are required.");

    var user = new IdentityUser { UserName = model.Username, Email = model.Email };
    var result = await userManager.CreateAsync(user, model.Password);

    return result.Succeeded
        ? Results.Ok(new { Message = "User registered successfully!" })
        : Results.BadRequest(result.Errors);
});

app.MapPost("/api/auth/login", async (UserManager<IdentityUser> userManager, IConfiguration config, LoginModel model) =>
{
    if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        return Results.BadRequest("Username and Password are required.");

    var user = await userManager.FindByNameAsync(model.Username);
    if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
    {
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            expires: DateTime.UtcNow.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return Results.Ok(new
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
        });
    }

    return Results.Unauthorized();
});

app.Run();

// Models
public record Snippet(int Id, string Language, string Code);
public record RegisterModel(string Username, string Email, string Password);
public record LoginModel(string Username, string Password);
