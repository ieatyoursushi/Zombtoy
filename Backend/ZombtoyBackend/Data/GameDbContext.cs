using Microsoft.EntityFrameworkCore;

namespace ZombtoyBackend.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<ScoreRow> Scores => Set<ScoreRow>();
}

public class ScoreRow
{
    public int Id { get; set; }
    public string Score { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
