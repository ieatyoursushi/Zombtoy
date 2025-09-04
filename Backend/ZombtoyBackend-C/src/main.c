#include <stdio.h>
#include <signal.h>
#include "mongoose.h"
#include "../include/models.h"

// Forward declarations
int init_database();
void close_database();
void http_handler(struct mg_connection* c, int ev, void* ev_data);

static int keep_running = 1;

// Signal handler for graceful shutdown
void signal_handler(int sig) {
    keep_running = 0;
}

int main() {
    printf("Starting Zombtoy C Backend...\n");

    // Initialize database
    if (init_database() != 0) {
        fprintf(stderr, "Failed to initialize database\n");
        return 1;
    }

    // Setup signal handler for Ctrl+C
    signal(SIGINT, signal_handler);

    // Initialize mongoose
    struct mg_mgr mgr;
    mg_mgr_init(&mgr);

    // Start HTTP server on port 8080
    printf("Starting HTTP server on http://0.0.0.0:8080\n");
    mg_http_listen(&mgr, "http://0.0.0.0:8080", http_handler, NULL);

    printf("Server is running. Press Ctrl+C to stop.\n");

    // Event loop
    while (keep_running) {
        mg_mgr_poll(&mgr, 1000);  // Poll for 1 second
    }

    // Cleanup
    printf("\nShutting down server...\n");
    mg_mgr_free(&mgr);
    close_database();
    printf("Server stopped.\n");

    return 0;
}
