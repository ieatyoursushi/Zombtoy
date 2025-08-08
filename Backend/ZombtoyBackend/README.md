Zombtoy Minimal API Backend (CSV -> SQLite)

What it does
- Keeps the same endpoints as the original Node/CSV version.
- Stores scores in a local SQLite database file (zombtoy.db) in this folder.

Endpoints
- GET / → text greeting
- POST /addScore → accepts text/plain (e.g., "1234") or JSON {"score":"1234"}; responds with "score received and stored: <score>"
- GET /getAllScores → returns a comma-separated list of score values (e.g., "100,200,300"). This matches Leaderboard.cs expectations.

Run locally
1) Install .NET 8 SDK
2) From this folder:
   - dotnet restore
   - dotnet run --urls "http://localhost:3000"

Quick test (fish / bash)
- curl -s -X POST http://localhost:3000/addScore -H 'Content-Type: text/plain' --data '999'
- curl -s http://localhost:3000/getAllScores

Notes
- The database file path is logged at startup.
- No PostgreSQL or external DB required for this minimal setup.

