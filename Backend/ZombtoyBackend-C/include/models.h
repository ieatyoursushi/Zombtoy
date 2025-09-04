#ifndef MODELS_H
#define MODELS_H

#include <time.h>

// ScoreRow struct equivalent to .NET ScoreRow although flawed score is currently a string for backwards compatibility.
typedef struct {
    int id;
    char score[256];  // Fixed-size string for simplicity
    time_t created_at;  // Unix timestamp
} ScoreRow;

#endif // MODELS_H
