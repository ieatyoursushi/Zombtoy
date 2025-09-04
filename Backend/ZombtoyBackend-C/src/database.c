#include <sqlite3.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "../include/models.h"

sqlite3* db = NULL;

// Initialize database and create table
int init_database() {
    int rc = sqlite3_open("zombtoy_c.db", &db);
    if (rc != SQLITE_OK) {
        fprintf(stderr, "Cannot open database: %s\n", sqlite3_errmsg(db));
        return rc;
    }

    const char* sql = "CREATE TABLE IF NOT EXISTS scores ("
                      "id INTEGER PRIMARY KEY AUTOINCREMENT, "
                      "score TEXT NOT NULL, "
                      "created_at INTEGER NOT NULL);";

    char* err_msg = NULL;
    rc = sqlite3_exec(db, sql, NULL, NULL, &err_msg);
    if (rc != SQLITE_OK) {
        fprintf(stderr, "SQL error: %s\n", err_msg);
        sqlite3_free(err_msg);
        return rc;
    }

    printf("Database initialized successfully\n");
    return SQLITE_OK;
}

// Add a score to the database
int add_score(const char* score) {
    if (!db) return SQLITE_ERROR;
    
    const char* sql = "INSERT INTO scores (score, created_at) VALUES (?, ?);";
    sqlite3_stmt* stmt;

    int rc = sqlite3_prepare_v2(db, sql, -1, &stmt, NULL);
    if (rc != SQLITE_OK) {
        fprintf(stderr, "Failed to prepare statement: %s\n", sqlite3_errmsg(db));
        return rc;
    }

    // Bind parameters
    sqlite3_bind_text(stmt, 1, score, -1, SQLITE_STATIC);
    sqlite3_bind_int64(stmt, 2, time(NULL));  // Current timestamp

    rc = sqlite3_step(stmt);
    if (rc != SQLITE_DONE) {
        fprintf(stderr, "Execution failed: %s\n", sqlite3_errmsg(db));
    }

    sqlite3_finalize(stmt);
    return rc == SQLITE_DONE ? SQLITE_OK : rc;
}

// Get all scores as comma-separated string
char* get_all_scores() {
    if (!db) return NULL;

    const char* sql = "SELECT score FROM scores ORDER BY id;";
    sqlite3_stmt* stmt;

    int rc = sqlite3_prepare_v2(db, sql, -1, &stmt, NULL);
    if (rc != SQLITE_OK) {
        fprintf(stderr, "Failed to prepare statement: %s\n", sqlite3_errmsg(db));
        return NULL;
    }

    // Estimate buffer size (rough)
    size_t buffer_size = 1024;
    char* result = malloc(buffer_size);
    if (!result) return NULL;

    result[0] = '\0';
    int first = 1;

    while ((rc = sqlite3_step(stmt)) == SQLITE_ROW) {
        const char* score = (const char*)sqlite3_column_text(stmt, 0);
        if (score) {
            size_t current_len = strlen(result);
            size_t score_len = strlen(score);

            // Resize buffer if needed
            if (current_len + score_len + 2 >= buffer_size) {
                buffer_size *= 2;
                char* new_result = realloc(result, buffer_size);
                if (!new_result) {
                    free(result);
                    sqlite3_finalize(stmt);
                    return NULL;
                }
                result = new_result;
            }

            if (!first) {
                strcat(result, ",");
            }
            strcat(result, score);
            first = 0;
        }
    }

    sqlite3_finalize(stmt);

    if (rc != SQLITE_DONE) {
        fprintf(stderr, "Query failed: %s\n", sqlite3_errmsg(db));
        free(result);
        return NULL;
    }

    return result;  // Caller must free this
}

// Close database
void close_database() {
    if (db) {
        sqlite3_close(db);
        db = NULL;
    }
}
