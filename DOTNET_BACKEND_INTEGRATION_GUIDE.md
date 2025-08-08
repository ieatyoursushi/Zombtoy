# Zombtoy Project - .NET Minimal API Backend Integration Guide

## Overview
This guide outlines the complete integration of a .NET Minimal API backend with the Zombtoy Unity game project. This will replace the current Node.js backend and provide a more robust, scalable, and type-safe solution for multiplayer gaming, high scores, user authentication, and real-time features.

---

## 🎯 Why .NET Minimal API for Game Backend?

### **Advantages Over Node.js**
- **Type Safety**: Compile-time error checking vs runtime errors
- **Performance**: Better performance for game logic calculations
- **Ecosystem**: Rich gaming libraries and SignalR for real-time features
- **Tooling**: Excellent debugging and profiling tools
- **Deployment**: Easy containerization and cloud deployment
- **Unity Integration**: Same C# language, easier data model sharing

### **Minimal API Benefits**
- **Less Boilerplate**: Faster development than traditional MVC
- **Modern Syntax**: Clean, readable code
- **Performance**: Optimized for high-throughput scenarios
- **Simple Scaling**: Easy to add complexity when needed

---

## 🏗️ Project Structure

```
ZombtoyBackend/
├── ZombtoyBackend.sln
├── src/
│   └── ZombtoyBackend/
│       ├── Program.cs                 # Main entry point
│       ├── Models/                    # Data models
│       │   ├── Player.cs
│       │   ├── GameSession.cs
│       │   ├── Score.cs
│       │   └── GameState.cs
│       ├── Services/                  # Business logic
│       │   ├── IPlayerService.cs
│       │   ├── PlayerService.cs
│       │   ├── IGameService.cs
│       │   ├── GameService.cs
│       │   └── IScoreService.cs
│       ├── Data/                      # Database context
│       │   ├── GameDbContext.cs
│       │   └── Repositories/
│       ├── Hubs/                      # SignalR hubs
│       │   ├── GameHub.cs
│       │   └── LobbyHub.cs
│       ├── Middleware/                # Custom middleware
│       │   ├── ErrorHandlingMiddleware.cs
│       │   └── RateLimitingMiddleware.cs
│       ├── Extensions/                # Extension methods
│       │   └── ServiceCollectionExtensions.cs
│       └── appsettings.json          # Configuration
└── tests/
    └── ZombtoyBackend.Tests/
```

---

## 🚀 Step-by-Step Implementation

### **Step 1: Create New .NET Project**

```bash
# Create solution and project
dotnet new sln -n ZombtoyBackend
dotnet new web -n ZombtoyBackend -f net8.0
dotnet sln add ZombtoyBackend/ZombtoyBackend.csproj

# Add required packages
cd ZombtoyBackend
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.SignalR
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Cors
dotnet add package Serilog.AspNetCore
dotnet add package FluentValidation.AspNetCore
```

### **Step 2: Define Data Models**

#### **Player.cs**
```csharp
namespace ZombtoyBackend.Models;

public class Player
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; }
    public bool IsOnline { get; set; }
    
    // Game-specific properties
    public int HighScore { get; set; }
    public int GamesPlayed { get; set; }
    public int TotalKills { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    
    // Navigation properties
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
```

#### **Score.cs**
```csharp
namespace ZombtoyBackend.Models;

public class Score
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int Value { get; set; }
    public int Level { get; set; }
    public int Kills { get; set; }
    public TimeSpan SurvivalTime { get; set; }
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    public string GameMode { get; set; } = "Survival";
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}
```

#### **GameSession.cs**
```csharp
namespace ZombtoyBackend.Models;

public class GameSession
{
    public int Id { get; set; }
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public int HostPlayerId { get; set; }
    public string GameMode { get; set; } = "Survival";
    public int MaxPlayers { get; set; } = 4;
    public int CurrentPlayers { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    
    // Navigation properties
    public Player HostPlayer { get; set; } = null!;
    public ICollection<GameSessionPlayer> Players { get; set; } = new List<GameSessionPlayer>();
}

public class GameSessionPlayer
{
    public int GameSessionId { get; set; }
    public int PlayerId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public GameSession GameSession { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
```

#### **GameState.cs (For Real-time Sync)**
```csharp
namespace ZombtoyBackend.Models;

public class GameState
{
    public string SessionId { get; set; } = string.Empty;
    public int CurrentWave { get; set; }
    public float GameTime { get; set; }
    public bool IsGameActive { get; set; }
    public List<PlayerState> Players { get; set; } = new();
    public List<EnemyState> Enemies { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class PlayerState
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float RotationY { get; set; }
    public int Health { get; set; }
    public float Stamina { get; set; }
    public int ActiveWeaponIndex { get; set; }
    public bool IsAlive { get; set; } = true;
}

public class EnemyState
{
    public int EnemyId { get; set; }
    public string EnemyType { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public int Health { get; set; }
    public bool IsAlive { get; set; } = true;
}
```

### **Step 3: Database Context**

#### **GameDbContext.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using ZombtoyBackend.Models;

namespace ZombtoyBackend.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<Player> Players { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }
    public DbSet<GameSessionPlayer> GameSessionPlayers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Player configuration
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
        });

        // Score configuration
        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany(e => e.Scores)
                  .HasForeignKey(e => e.PlayerId);
            entity.HasIndex(e => new { e.Value, e.AchievedAt });
        });

        // GameSession configuration
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasOne(e => e.HostPlayer)
                  .WithMany(e => e.GameSessions)
                  .HasForeignKey(e => e.HostPlayerId);
        });

        // GameSessionPlayer configuration
        modelBuilder.Entity<GameSessionPlayer>(entity =>
        {
            entity.HasKey(e => new { e.GameSessionId, e.PlayerId });
            entity.HasOne(e => e.GameSession)
                  .WithMany(e => e.Players)
                  .HasForeignKey(e => e.GameSessionId);
            entity.HasOne(e => e.Player)
                  .WithMany()
                  .HasForeignKey(e => e.PlayerId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
```

### **Step 4: Services Layer**

#### **IPlayerService.cs**
```csharp
using ZombtoyBackend.Models;

namespace ZombtoyBackend.Services;

public interface IPlayerService
{
    Task<Player?> GetPlayerByIdAsync(int id);
    Task<Player?> GetPlayerByUsernameAsync(string username);
    Task<Player> CreatePlayerAsync(string username, string email, string password);
    Task<Player?> AuthenticatePlayerAsync(string username, string password);
    Task<bool> UpdatePlayerStatsAsync(int playerId, int kills, TimeSpan playTime);
    Task<bool> UpdatePlayerOnlineStatusAsync(int playerId, bool isOnline);
}
```

#### **PlayerService.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using ZombtoyBackend.Data;
using ZombtoyBackend.Models;
using System.Security.Cryptography;
using System.Text;

namespace ZombtoyBackend.Services;

public class PlayerService : IPlayerService
{
    private readonly GameDbContext _context;

    public PlayerService(GameDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetPlayerByIdAsync(int id)
    {
        return await _context.Players
            .Include(p => p.Scores.OrderByDescending(s => s.Value).Take(10))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Player?> GetPlayerByUsernameAsync(string username)
    {
        return await _context.Players
            .FirstOrDefaultAsync(p => p.Username == username);
    }

    public async Task<Player> CreatePlayerAsync(string username, string email, string password)
    {
        var player = new Player
        {
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task<Player?> AuthenticatePlayerAsync(string username, string password)
    {
        var player = await GetPlayerByUsernameAsync(username);
        
        if (player == null || !VerifyPassword(password, player.PasswordHash))
            return null;

        player.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return player;
    }

    public async Task<bool> UpdatePlayerStatsAsync(int playerId, int kills, TimeSpan playTime)
    {
        var player = await GetPlayerByIdAsync(playerId);
        if (player == null) return false;

        player.TotalKills += kills;
        player.TotalPlayTime += playTime;
        player.GamesPlayed++;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePlayerOnlineStatusAsync(int playerId, bool isOnline)
    {
        var player = await GetPlayerByIdAsync(playerId);
        if (player == null) return false;

        player.IsOnline = isOnline;
        await _context.SaveChangesAsync();
        return true;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "ZombtoyGameSalt"));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
```

#### **IScoreService.cs**
```csharp
using ZombtoyBackend.Models;

namespace ZombtoyBackend.Services;

public interface IScoreService
{
    Task<Score> SubmitScoreAsync(int playerId, int value, int level, int kills, TimeSpan survivalTime, string gameMode = "Survival");
    Task<List<Score>> GetLeaderboardAsync(string gameMode = "Survival", int limit = 10);
    Task<List<Score>> GetPlayerScoresAsync(int playerId, int limit = 10);
    Task<bool> ValidateScoreAsync(Score score);
}
```

#### **ScoreService.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using ZombtoyBackend.Data;
using ZombtoyBackend.Models;

namespace ZombtoyBackend.Services;

public class ScoreService : IScoreService
{
    private readonly GameDbContext _context;
    private readonly IPlayerService _playerService;

    public ScoreService(GameDbContext context, IPlayerService playerService)
    {
        _context = context;
        _playerService = playerService;
    }

    public async Task<Score> SubmitScoreAsync(int playerId, int value, int level, int kills, TimeSpan survivalTime, string gameMode = "Survival")
    {
        var score = new Score
        {
            PlayerId = playerId,
            Value = value,
            Level = level,
            Kills = kills,
            SurvivalTime = survivalTime,
            GameMode = gameMode,
            AchievedAt = DateTime.UtcNow
        };

        // Validate score before saving
        if (!await ValidateScoreAsync(score))
        {
            throw new InvalidOperationException("Invalid score submission");
        }

        _context.Scores.Add(score);
        await _context.SaveChangesAsync();

        // Update player's high score if necessary
        var player = await _playerService.GetPlayerByIdAsync(playerId);
        if (player != null && value > player.HighScore)
        {
            player.HighScore = value;
            await _context.SaveChangesAsync();
        }

        return score;
    }

    public async Task<List<Score>> GetLeaderboardAsync(string gameMode = "Survival", int limit = 10)
    {
        return await _context.Scores
            .Include(s => s.Player)
            .Where(s => s.GameMode == gameMode)
            .OrderByDescending(s => s.Value)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Score>> GetPlayerScoresAsync(int playerId, int limit = 10)
    {
        return await _context.Scores
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.Value)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<bool> ValidateScoreAsync(Score score)
    {
        // Basic validation rules
        if (score.Value < 0 || score.Level < 1 || score.Kills < 0)
            return false;

        // Anti-cheat: Check if score is reasonable for the survival time
        var maxScorePerSecond = 100; // Adjust based on game mechanics
        var maxPossibleScore = (int)(score.SurvivalTime.TotalSeconds * maxScorePerSecond);
        
        if (score.Value > maxPossibleScore)
            return false;

        // Check if player exists
        var player = await _playerService.GetPlayerByIdAsync(score.PlayerId);
        return player != null;
    }
}
```

### **Step 5: SignalR Hubs for Real-time Features**

#### **GameHub.cs**
```csharp
using Microsoft.AspNetCore.SignalR;
using ZombtoyBackend.Models;
using ZombtoyBackend.Services;

namespace ZombtoyBackend.Hubs;

public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private static readonly Dictionary<string, GameState> _activeGames = new();

    public GameHub(IGameService gameService)
    {
        _gameService = gameService;
    }

    public async Task JoinGame(string sessionId, int playerId, string username)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        
        if (!_activeGames.ContainsKey(sessionId))
        {
            _activeGames[sessionId] = new GameState { SessionId = sessionId };
        }

        var gameState = _activeGames[sessionId];
        var existingPlayer = gameState.Players.FirstOrDefault(p => p.PlayerId == playerId);
        
        if (existingPlayer == null)
        {
            gameState.Players.Add(new PlayerState 
            { 
                PlayerId = playerId, 
                Username = username,
                Health = 100,
                Stamina = 1.0f,
                IsAlive = true
            });
        }
        else
        {
            existingPlayer.IsAlive = true;
        }

        await Clients.Group(sessionId).SendAsync("PlayerJoined", playerId, username);
        await Clients.Caller.SendAsync("GameStateUpdate", gameState);
    }

    public async Task LeaveGame(string sessionId, int playerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        
        if (_activeGames.ContainsKey(sessionId))
        {
            var gameState = _activeGames[sessionId];
            var player = gameState.Players.FirstOrDefault(p => p.PlayerId == playerId);
            if (player != null)
            {
                gameState.Players.Remove(player);
            }

            if (gameState.Players.Count == 0)
            {
                _activeGames.Remove(sessionId);
            }
        }

        await Clients.Group(sessionId).SendAsync("PlayerLeft", playerId);
    }

    public async Task UpdatePlayerPosition(string sessionId, int playerId, float x, float y, float z, float rotY)
    {
        if (_activeGames.ContainsKey(sessionId))
        {
            var gameState = _activeGames[sessionId];
            var player = gameState.Players.FirstOrDefault(p => p.PlayerId == playerId);
            
            if (player != null)
            {
                player.PositionX = x;
                player.PositionY = y;
                player.PositionZ = z;
                player.RotationY = rotY;
            }
        }

        await Clients.OthersInGroup(sessionId).SendAsync("PlayerPositionUpdate", playerId, x, y, z, rotY);
    }

    public async Task UpdatePlayerHealth(string sessionId, int playerId, int health)
    {
        if (_activeGames.ContainsKey(sessionId))
        {
            var gameState = _activeGames[sessionId];
            var player = gameState.Players.FirstOrDefault(p => p.PlayerId == playerId);
            
            if (player != null)
            {
                player.Health = health;
                if (health <= 0)
                {
                    player.IsAlive = false;
                }
            }
        }

        await Clients.Group(sessionId).SendAsync("PlayerHealthUpdate", playerId, health);
    }

    public async Task PlayerFired(string sessionId, int playerId, float dirX, float dirY, float dirZ)
    {
        await Clients.OthersInGroup(sessionId).SendAsync("PlayerFired", playerId, dirX, dirY, dirZ);
    }

    public async Task EnemyKilled(string sessionId, int enemyId, int killedByPlayerId)
    {
        if (_activeGames.ContainsKey(sessionId))
        {
            var gameState = _activeGames[sessionId];
            var enemy = gameState.Enemies.FirstOrDefault(e => e.EnemyId == enemyId);
            if (enemy != null)
            {
                gameState.Enemies.Remove(enemy);
            }
        }

        await Clients.Group(sessionId).SendAsync("EnemyKilled", enemyId, killedByPlayerId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Handle cleanup when player disconnects unexpectedly
        await base.OnDisconnectedAsync(exception);
    }
}
```

### **Step 6: Main Program.cs (Minimal API)**

#### **Program.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ZombtoyBackend.Data;
using ZombtoyBackend.Services;
using ZombtoyBackend.Hubs;
using ZombtoyBackend.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<IGameService, GameService>();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://your-unity-game-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? ""))
        };

        // Allow JWT in SignalR connections
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/gamehub"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    context.Database.EnsureCreated();
}

// Authentication endpoints
app.MapPost("/auth/register", async (RegisterRequest request, IPlayerService playerService) =>
{
    try
    {
        var player = await playerService.CreatePlayerAsync(request.Username, request.Email, request.Password);
        var token = GenerateJwtToken(player);
        
        return Results.Ok(new AuthResponse
        {
            Success = true,
            Token = token,
            Player = new PlayerDto
            {
                Id = player.Id,
                Username = player.Username,
                Email = player.Email,
                HighScore = player.HighScore
            }
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
});

app.MapPost("/auth/login", async (LoginRequest request, IPlayerService playerService) =>
{
    var player = await playerService.AuthenticatePlayerAsync(request.Username, request.Password);
    
    if (player == null)
    {
        return Results.Unauthorized();
    }
    
    await playerService.UpdatePlayerOnlineStatusAsync(player.Id, true);
    var token = GenerateJwtToken(player);
    
    return Results.Ok(new AuthResponse
    {
        Success = true,
        Token = token,
        Player = new PlayerDto
        {
            Id = player.Id,
            Username = player.Username,
            Email = player.Email,
            HighScore = player.HighScore,
            GamesPlayed = player.GamesPlayed,
            TotalKills = player.TotalKills
        }
    });
});

// Game endpoints
app.MapGet("/", () => "Zombtoy Game Backend API - Running!");

app.MapGet("/leaderboard", async (IScoreService scoreService, string gameMode = "Survival") =>
{
    var scores = await scoreService.GetLeaderboardAsync(gameMode);
    return Results.Ok(scores.Select(s => new LeaderboardEntry
    {
        Username = s.Player.Username,
        Score = s.Value,
        Level = s.Level,
        Kills = s.Kills,
        SurvivalTime = s.SurvivalTime,
        AchievedAt = s.AchievedAt
    }));
});

app.MapPost("/scores", async (SubmitScoreRequest request, IScoreService scoreService, ClaimsPrincipal user) =>
{
    if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int playerId))
    {
        return Results.Unauthorized();
    }

    try
    {
        var score = await scoreService.SubmitScoreAsync(
            playerId, 
            request.Score, 
            request.Level, 
            request.Kills, 
            TimeSpan.FromSeconds(request.SurvivalTimeSeconds),
            request.GameMode);
            
        return Results.Ok(new { Success = true, ScoreId = score.Id });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
}).RequireAuthorization();

app.MapGet("/player/{id}/scores", async (int id, IScoreService scoreService) =>
{
    var scores = await scoreService.GetPlayerScoresAsync(id);
    return Results.Ok(scores);
});

app.MapGet("/player/profile", async (IPlayerService playerService, ClaimsPrincipal user) =>
{
    if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int playerId))
    {
        return Results.Unauthorized();
    }

    var player = await playerService.GetPlayerByIdAsync(playerId);
    if (player == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new PlayerDto
    {
        Id = player.Id,
        Username = player.Username,
        Email = player.Email,
        HighScore = player.HighScore,
        GamesPlayed = player.GamesPlayed,
        TotalKills = player.TotalKills,
        TotalPlayTime = player.TotalPlayTime
    });
}).RequireAuthorization();

// SignalR Hub
app.MapHub<GameHub>("/gamehub");

app.Run();

// Helper methods and DTOs
string GenerateJwtToken(Player player)
{
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"] ?? "");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.Name, player.Username)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        Issuer = builder.Configuration["Jwt:Issuer"],
        Audience = builder.Configuration["Jwt:Audience"]
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

// Request/Response DTOs
public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Username, string Password);
public record SubmitScoreRequest(int Score, int Level, int Kills, double SurvivalTimeSeconds, string GameMode = "Survival");

public class AuthResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public PlayerDto? Player { get; set; }
}

public class PlayerDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int HighScore { get; set; }
    public int GamesPlayed { get; set; }
    public int TotalKills { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
}

public class LeaderboardEntry
{
    public string Username { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Level { get; set; }
    public int Kills { get; set; }
    public TimeSpan SurvivalTime { get; set; }
    public DateTime AchievedAt { get; set; }
}
```

### **Step 7: Configuration (appsettings.json)**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=zombtoy.db"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyThatShouldBeAtLeast256BitsLong!",
    "Issuer": "ZombtoyGameBackend",
    "Audience": "ZombtoyGameClients"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🎮 Unity Integration

### **Step 1: Unity HTTP Client**

```csharp
// GameBackendClient.cs
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class GameBackendClient : MonoBehaviour
{
    private const string BASE_URL = "http://localhost:5000"; // Change to your server URL
    private string authToken = "";
    
    public static GameBackendClient Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public IEnumerator RegisterPlayer(string username, string email, string password, System.Action<bool, string> callback)
    {
        var request = new RegisterRequest { Username = username, Email = email, Password = password };
        var json = JsonConvert.SerializeObject(request);
        
        using (var www = UnityWebRequest.Put($"{BASE_URL}/auth/register", json))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<AuthResponse>(www.downloadHandler.text);
                if (response.Success)
                {
                    authToken = response.Token;
                    callback(true, "Registration successful!");
                }
                else
                {
                    callback(false, "Registration failed");
                }
            }
            else
            {
                callback(false, www.error);
            }
        }
    }
    
    public IEnumerator LoginPlayer(string username, string password, System.Action<bool, string> callback)
    {
        var request = new LoginRequest { Username = username, Password = password };
        var json = JsonConvert.SerializeObject(request);
        
        using (var www = UnityWebRequest.Put($"{BASE_URL}/auth/login", json))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<AuthResponse>(www.downloadHandler.text);
                if (response.Success)
                {
                    authToken = response.Token;
                    callback(true, "Login successful!");
                }
                else
                {
                    callback(false, "Invalid credentials");
                }
            }
            else
            {
                callback(false, www.error);
            }
        }
    }
    
    public IEnumerator SubmitScore(int score, int level, int kills, float survivalTime, System.Action<bool, string> callback)
    {
        var request = new SubmitScoreRequest 
        { 
            Score = score, 
            Level = level, 
            Kills = kills, 
            SurvivalTimeSeconds = survivalTime 
        };
        var json = JsonConvert.SerializeObject(request);
        
        using (var www = UnityWebRequest.Put($"{BASE_URL}/scores", json))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {authToken}");
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                callback(true, "Score submitted!");
            }
            else
            {
                callback(false, www.error);
            }
        }
    }
    
    public IEnumerator GetLeaderboard(System.Action<LeaderboardEntry[]> callback)
    {
        using (var www = UnityWebRequest.Get($"{BASE_URL}/leaderboard"))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                var leaderboard = JsonConvert.DeserializeObject<LeaderboardEntry[]>(www.downloadHandler.text);
                callback(leaderboard);
            }
            else
            {
                callback(new LeaderboardEntry[0]);
            }
        }
    }
}

// Data classes matching the backend DTOs
[System.Serializable]
public class RegisterRequest
{
    public string Username;
    public string Email;
    public string Password;
}

[System.Serializable]
public class LoginRequest
{
    public string Username;
    public string Password;
}

[System.Serializable]
public class SubmitScoreRequest
{
    public int Score;
    public int Level;
    public int Kills;
    public double SurvivalTimeSeconds;
    public string GameMode = "Survival";
}

[System.Serializable]
public class AuthResponse
{
    public bool Success;
    public string Token;
    public PlayerDto Player;
}

[System.Serializable]
public class PlayerDto
{
    public int Id;
    public string Username;
    public string Email;
    public int HighScore;
    public int GamesPlayed;
    public int TotalKills;
}

[System.Serializable]
public class LeaderboardEntry
{
    public string Username;
    public int Score;
    public int Level;
    public int Kills;
    public System.TimeSpan SurvivalTime;
    public System.DateTime AchievedAt;
}
```

### **Step 2: Updated PlayerHealth Integration**

```csharp
// Update your existing PlayerHealth.cs to integrate with the backend
public class PlayerHealth : MonoBehaviour
{
    // ... existing code ...
    
    // Add these new fields
    private int gameStartKills = 0;
    private float gameStartTime;
    private bool gameEnded = false;
    
    void Start()
    {
        gameStartTime = Time.time;
        gameStartKills = 0; // Reset kill count
        gameEnded = false;
    }
    
    // Modified Death method
    void Death()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            
            // Calculate game stats
            float survivalTime = Time.time - gameStartTime;
            int killsThisGame = GetCurrentKillCount() - gameStartKills;
            int finalScore = CalculateFinalScore();
            int currentLevel = GetCurrentLevel();
            
            // Submit score to backend
            StartCoroutine(SubmitGameScore(finalScore, currentLevel, killsThisGame, survivalTime));
        }
        
        // ... existing death logic ...
    }
    
    private IEnumerator SubmitGameScore(int score, int level, int kills, float survivalTime)
    {
        yield return GameBackendClient.Instance.SubmitScore(score, level, kills, survivalTime, (success, message) =>
        {
            if (success)
            {
                Debug.Log("Score submitted successfully!");
            }
            else
            {
                Debug.LogError($"Failed to submit score: {message}");
            }
        });
    }
    
    // Helper methods - implement based on your game logic
    private int CalculateFinalScore()
    {
        // Return the final score based on your game's scoring system
        return 0; // Replace with actual calculation
    }
    
    private int GetCurrentLevel()
    {
        // Return the current level/wave
        return 1; // Replace with actual level tracking
    }
    
    private int GetCurrentKillCount()
    {
        // Return the current kill count
        return 0; // Replace with actual kill tracking
    }
}
```

---

## 🚀 Deployment & Production

### **Step 1: Docker Support**

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ZombtoyBackend.csproj", "."]
RUN dotnet restore "ZombtoyBackend.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "ZombtoyBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZombtoyBackend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZombtoyBackend.dll"]
```

### **Step 2: Production Configuration**

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-db-server;Database=zombtoy;User Id=your-user;Password=your-password;"
  },
  "Jwt": {
    "Secret": "YourProductionSecretKey256Bits",
    "Issuer": "ZombtoyGameBackend",
    "Audience": "ZombtoyGameClients"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 📊 Performance & Scaling Considerations

### **Database Optimization**
- Add proper indexes for leaderboard queries
- Implement connection pooling
- Consider read replicas for leaderboards

### **Real-time Optimization**
- Use Redis for game state caching
- Implement connection pooling for SignalR
- Add rate limiting for real-time events

### **Security**
- Implement proper input validation
- Add rate limiting middleware
- Use HTTPS in production
- Implement anti-cheat measures

---

## ✅ Migration Checklist

- [ ] Set up .NET 8 development environment
- [ ] Create new .NET Minimal API project
- [ ] Implement data models and database context
- [ ] Create service layer (Player, Score, Game)
- [ ] Implement JWT authentication
- [ ] Add SignalR hubs for real-time features
- [ ] Create Unity HTTP client integration
- [ ] Update existing Unity scripts to use new backend
- [ ] Test authentication flow
- [ ] Test score submission and leaderboards
- [ ] Test real-time multiplayer features
- [ ] Deploy to production environment
- [ ] Monitor and optimize performance

---

*This guide provides a complete foundation for migrating from Node.js to a robust .NET Minimal API backend that will scale with your game's growth and provide excellent performance for multiplayer gaming scenarios.*
