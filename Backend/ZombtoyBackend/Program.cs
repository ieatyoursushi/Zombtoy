using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ZombtoyBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
// Configure SQLite database (local file zombtoy.db by default)
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "zombtoy.db");
builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

var app = builder.Build();

Console.WriteLine($"[ZombtoyBackend] Using SQLite db at: {dbPath}");

// Ensure database exists with the Scores table
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => Results.Text("Welcome to root URL of Server"));

// Mirrors previous Node behavior: request body is plain text score; append as JSON object with trailing comma
app.MapPost("/addScore", async (HttpContext ctx, GameDbContext db) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync();
    var trimmed = body?.Trim() ?? string.Empty;
    if (string.IsNullOrEmpty(trimmed))
    {
        return Results.BadRequest("empty body");
    }
    // Accept either raw text (e.g., "100") or JSON {"score":"100"}
    string score;
    if (trimmed.StartsWith("{"))
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<ScoreJson>(trimmed);
            score = parsed?.score?.Trim() ?? string.Empty;
        }
        catch
        {
            return Results.BadRequest("invalid JSON");
        }
    }
    else
    {
        score = trimmed;
    }
    if (string.IsNullOrWhiteSpace(score))
    {
        return Results.BadRequest("missing score");
    }
    try
    {
    // Store as a row (string score), preserving the simple model
    await db.Scores.AddAsync(new ScoreRow { Score = score });
    await db.SaveChangesAsync();
        return Results.Text($"score received and stored: {score}");
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 422, title: "failed to store score");
    }
});

app.MapGet("/getAllScores", async (GameDbContext db) =>
{
    try
    {
    var scoresList = await db.Scores.AsNoTracking().OrderBy(s => s.Id).Select(s => s.Score).ToListAsync();
    var scores = string.Join(',', scoresList);
        return Results.Text(scores);
    }
    catch
    {
        return Results.Text(string.Empty);
    }
});

app.Run();

public record ScoreJson([property: JsonPropertyName("score")] string score);
