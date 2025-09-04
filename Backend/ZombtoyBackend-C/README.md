# ZombtoyBackend-C

A primitive C-based backend for the Zombtoy game, equivalent to the .NET version so far but implemented from scratch for educational purposes. This demonstrates low-level backend development, memory management, and direct database interactions.

## Overview

This is a learning project to understand what's "under the hood" of high-level frameworks like .NET's ASP.NET Core and Entity Framework. It implements a simple score system using:

- **SQLite** for data persistence (direct C API, no ORM)
- **mongoose** for HTTP server (lightweight, single-header library)
- **Manual memory management** (malloc/free, no garbage collection)
- **Raw socket programming** concepts (abstracted by mongoose)

## Project Structure

```
ZombtoyBackend-C/
├── src/
│   ├── main.c              # Entry point, server setup
│   ├── database.c          # SQLite operations
│   └── http_handlers.c     # HTTP request handling
├── include/
│   └── models.h            # Data structures
├── mongoose.h              # HTTP library (single header)
├── Makefile                # Build configuration
├── C-Mem-Management-Refresher.md  # Memory management guide
└── README.md               # This file
```

## Prerequisites

### macOS (with Homebrew)
```bash
# Install SQLite (already done)
brew install sqlite3

# Set environment variables for compilation
set -gx LDFLAGS "-L/opt/homebrew/opt/sqlite/lib"
set -gx CPPFLAGS "-I/opt/homebrew/opt/sqlite/include"
```

### Linux
```bash
sudo apt-get install libsqlite3-dev
```

### Windows
- Install SQLite from https://www.sqlite.org/download.html
- Use MinGW or similar for compilation

## Building and Running

### 1. Build the Project
```bash
make
```

### 2. Run the Server
```bash
make run
# or
./zombtoy_backend_c
```

The server will start on `http://localhost:8080`.

### 3. Test Endpoints

#### Add a Score
```bash
# Plain text
curl -X POST http://localhost:8080/addScore -d "150"

# JSON format
curl -X POST http://localhost:8080/addScore \
  -H "Content-Type: application/json" \
  -d '{"score":"200"}'
```

#### Get All Scores
```bash
curl http://localhost:8080/getAllScores
```

#### Welcome Message
```bash
curl http://localhost:8080/
```

### 4. Automated Testing
```bash
make test
```

### 5. Clean Build
```bash
make clean
```

## API Endpoints

| Method | Endpoint | Description | Example |
|--------|----------|-------------|---------|
| GET | `/` | Welcome message | `curl http://localhost:8080/` |
| POST | `/addScore` | Add a score | `curl -X POST http://localhost:8080/addScore -d "100"` |
| GET | `/getAllScores` | Get all scores (comma-separated) | `curl http://localhost:8080/getAllScores` |

## Key Differences from .NET Version

### Memory Management
- **.NET**: Automatic GC, `new` keyword
- **C**: Manual `malloc`/`free`, explicit memory tracking
- **Learning**: Understand heap vs. stack, memory leaks, buffer overflows

### Database Layer
- **.NET**: Entity Framework ORM, LINQ queries
- **C**: Raw SQL with `sqlite3_exec()`, manual result parsing
- **Learning**: SQL injection prevention, prepared statements, connection management

### HTTP Server
- **.NET**: ASP.NET Core with Kestrel, automatic routing/middleware
- **C**: mongoose handles HTTP parsing, manual routing in C code
- **Learning**: HTTP protocol details, request/response formatting

### Error Handling
- **.NET**: Exceptions, try/catch
- **C**: Return codes, errno, manual error checking
- **Learning**: Defensive programming, resource cleanup

## Educational Goals

This backend-segment of Zombtoy helps you understand:
- How web frameworks abstract low-level networking
- Why ORMs exist (manual SQL is tedious and error-prone)
- The cost of manual memory management
- Real-world C programming patterns
- The value of high-level languages for productivity

## Comparison with .NET Backend

| Aspect | .NET Version | C Version |
|--------|--------------|-----------|
| Lines of Code | ~100 | ~300 |
| Build Time | Fast | Slower (compilation) |
| Runtime | Managed (.NET CLR) | Native (machine code) |
| Memory | GC-managed | Manual |
| Dependencies | Many NuGet packages | SQLite + mongoose |
| Learning Value | Framework usage | System internals |

## Next Steps

1. **Add Features**: Implement user authentication, score validation
2. **Performance**: Profile memory usage, optimize allocations
3. **Security**: Add input validation, prevent SQL injection
4. **Advanced**: Add threading for concurrent requests
5. **Compare**: Run both backends side-by-side, measure differences

## Troubleshooting

### Compilation Errors
- Ensure SQLite headers are found: Check `CPPFLAGS` and `LDFLAGS`
- On macOS: `brew info sqlite3` for paths

### Runtime Errors
- Database file permissions: `chmod 644 zombtoy_c.db`
- Port conflicts: Change port in `main.c` if 8080 is busy

### Memory Issues
- Use Valgrind to detect leaks: `valgrind ./zombtoy_backend_c`
- Check for uninitialized pointers and buffer overflows

## Memory Management & Pointer Applications

This C backend demonstrates various memory allocation patterns and pointer applications that are abstracted away in high-level languages like .NET. Understanding these concepts is crucial for low-level programming.

### Memory Allocation Types

#### 1. Dynamic Memory Allocation (`malloc`/`realloc`/`free`)
```c
// Dynamic buffer for database query results
char* result = malloc(buffer_size);           // Initial allocation
char* new_result = realloc(result, buffer_size); // Resize if needed
free(result);                                // Manual deallocation
```
- **Location**: `database.c` - `get_all_scores()` function
- **Purpose**: Handle variable-sized data from database queries
- **Pattern**: Start with estimated size, grow exponentially when needed
- **Risk**: Manual memory management - must track and free all allocations

#### 2. Static Memory Allocation (Stack-based)
```c
// Fixed-size buffers on stack
char body[1024] = {0};        // HTTP request body buffer
char score[256] = {0};        // Score parsing buffer
char response[512];           // HTTP response buffer
```
- **Location**: `http_handlers.c` - `http_handler()` function
- **Purpose**: Temporary buffers for HTTP request processing
- **Pattern**: Pre-allocated on stack, automatically freed when function exits
- **Advantage**: No manual cleanup needed, fast allocation

#### 3. Library-Managed Memory (Opaque Handles)
```c
// SQLite and mongoose library handles
sqlite3* db = NULL;                    // Database connection handle
sqlite3_stmt* stmt;                    // Prepared statement handle
struct mg_mgr mgr;                     // HTTP server manager
struct mg_connection* c;               // HTTP connection
```
- **Location**: Throughout `database.c` and `main.c`
- **Purpose**: Library-managed resources with internal memory allocation
- **Pattern**: Opaque pointers - internal memory managed by libraries

### Pointer Applications

#### 1. String Manipulation Pointers
```c
// JSON parsing with pointer arithmetic
char* score_start = strstr(body, "\"score\":");  // Find substring
char* score_end = strchr(score_start, '"');     // Find character
memcpy(score, score_start, len);                // Copy memory region
```
- **Location**: `http_handlers.c` - JSON parsing logic
- **Purpose**: Parse JSON and manipulate text data
- **Pattern**: Pointer arithmetic for efficient string processing

#### 2. Struct Pointers
```c
// HTTP message handling
struct mg_http_message* hm = (struct mg_http_message*)ev_data;
struct mg_connection* c;  // Connection pointer
```
- **Location**: `http_handlers.c` - Event handling
- **Purpose**: Access mongoose library structures
- **Pattern**: Cast void pointers to typed structs for type safety

#### 3. Function Pointers
```c
// Signal handling and callbacks
signal(SIGINT, signal_handler);  // Register signal callback
void (*http_handler)(struct mg_connection*, int, void*);  // HTTP handler
```
- **Location**: `main.c` - Signal handling and mongoose setup
- **Purpose**: Event-driven programming (signals, HTTP requests)
- **Pattern**: Callback registration for asynchronous events

#### 4. Database Handle Pointers
```c
// SQLite operations
sqlite3* db;              // Database connection
sqlite3_stmt* stmt;       // Prepared statement
const char* score = (const char*)sqlite3_column_text(stmt, 0);  // Result data
```
- **Location**: `database.c` - All database operations
- **Purpose**: Interface with SQLite C API
- **Pattern**: Opaque handles for database operations

#### 5. Array Pointers (Fixed-size)
```c
// Fixed-size string in struct
typedef struct {
    int id;
    char score[256];      // Array (decays to pointer)
    time_t created_at;
} ScoreRow;
```
- **Location**: `include/models.h` - Data structure definition
- **Purpose**: Fixed-size string storage in structs
- **Pattern**: Array notation that decays to pointers

### Memory Management Patterns

#### Manual Memory Lifecycle
```c
char* get_all_scores() {
    char* result = malloc(buffer_size);
    // ... populate result ...
    return result;  // Caller must free!
}
```
- **Pattern**: Allocate in one function, deallocate in another
- **Risk**: Memory leaks if caller forgets to free
- **Location**: `database.c` - `get_all_scores()` function

#### RAII-like Pattern (Manual)
```c
int add_score(const char* score) {
    sqlite3_stmt* stmt;
    sqlite3_prepare_v2(db, sql, -1, &stmt, NULL);
    // ... use stmt ...
    sqlite3_finalize(stmt);  // Clean up immediately
}
```
- **Pattern**: Allocate resource, use it, immediately clean up
- **Advantage**: No resource leaks within function scope
- **Location**: `database.c` - All database operations

#### Buffer Growth Strategy
```c
if (current_len + score_len + 2 >= buffer_size) {
    buffer_size *= 2;
    char* new_result = realloc(result, buffer_size);
}
```
- **Pattern**: Double buffer size when needed
- **Purpose**: Handle unknown data sizes efficiently
- **Location**: `database.c` - Dynamic buffer resizing

### Memory Safety Considerations

1. **Buffer Overflows**: Fixed-size arrays (`char score[256]`) could overflow
2. **Memory Leaks**: `malloc()` without corresponding `free()`
3. **Dangling Pointers**: Using freed memory
4. **Null Pointer Dereference**: Not checking pointers before use
5. **Double Free**: Freeing already freed memory

### Comparison with .NET

| Aspect | C Backend | .NET Backend |
|--------|-----------|--------------|
| **Memory Management** | Manual (malloc/free) | Automatic (GC) |
| **Error Handling** | Return codes | Exceptions |
| **String Handling** | Pointer arithmetic | String objects |
| **Database Access** | Raw SQL | ORM (EF) |
| **HTTP Server** | mongoose library | Kestrel |
| **Development Speed** | Slower, error-prone | Faster, safer |
| **Performance** | Potentially better | Good enough |
| **Learning Value** | Deep understanding | Framework usage |

This memory management analysis demonstrates the **low-level control** that C provides compared to .NET's high-level abstractions. Understanding these concepts helps you appreciate what modern frameworks do automatically!

## Resources

- [SQLite C API Documentation](https://www.sqlite.org/cintro.html)
- [mongoose Documentation](https://mongoose.ws/documentation/)
- [Beej's Guide to Network Programming](https://beej.us/guide/bgnet/)
- [C Memory Management](https://en.wikipedia.org/wiki/C_dynamic_memory_allocation)

Happy learning! This C backend will give you deep insights into what .NET does for you automatically. 🚀
