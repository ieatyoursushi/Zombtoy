#include "mongoose.h"
#include <string.h>
#include <stdio.h>
#include "../include/models.h"

// Forward declarations
int add_score(const char* score);
char* get_all_scores();

// HTTP request handler
void http_handler(struct mg_connection* c, int ev, void* ev_data) {
    if (ev == MG_EV_HTTP_MSG) {
        struct mg_http_message* hm = (struct mg_http_message*)ev_data;

        // Handle GET /
        if (mg_match(hm->uri, mg_str("/"), NULL)) {
            mg_http_reply(c, 200, "Content-Type: text/plain\r\n", "Welcome to Zombtoy C Backend\n");
            return;
        }

        // Handle POST /addScore
        if (mg_match(hm->uri, mg_str("/addScore"), NULL)) {
            // Get request body
            char body[1024] = {0};
            size_t body_len = hm->body.len < sizeof(body) - 1 ? hm->body.len : sizeof(body) - 1;
            memcpy(body, hm->body.buf, body_len);
            body[body_len] = '\0';

            // Parse score (simple: assume plain text or JSON {"score":"value"} or {"score":number})
            char score[256] = {0};
            if (body[0] == '{') {
                // Simple JSON parsing (not robust, for demo)
                char* score_start = strstr(body, "\"score\":");
                if (score_start) {
                    score_start += 8;  // Skip "score":
                    
                    // Skip whitespace
                    while (*score_start == ' ' || *score_start == '\t') score_start++;
                    
                    if (*score_start == '"') {
                        // String value
                        score_start++;  // Skip opening quote
                        char* score_end = strchr(score_start, '"');
                        if (score_end) {
                            size_t len = score_end - score_start;
                            if (len < sizeof(score)) {
                                memcpy(score, score_start, len);
                                score[len] = '\0';
                            }
                        }
                    } else {
                        // Number value
                        char* score_end = score_start;
                        while (*score_end >= '0' && *score_end <= '9') score_end++;
                        size_t len = score_end - score_start;
                        if (len < sizeof(score)) {
                            memcpy(score, score_start, len);
                            score[len] = '\0';
                        }
                    }
                }
            } else {
                // Plain text
                strncpy(score, body, sizeof(score) - 1);
            }

            if (strlen(score) > 0) {
                if (add_score(score) == 0) {
                    char response[512];
                    snprintf(response, sizeof(response), "score received and stored: %s", score);
                    mg_http_reply(c, 200, "Content-Type: text/plain\r\n", response);
                } else {
                    mg_http_reply(c, 500, "Content-Type: text/plain\r\n", "Failed to store score");
                }
            } else {
                mg_http_reply(c, 400, "Content-Type: text/plain\r\n", "Invalid score");
            }
            return;
        }

        // Handle GET /getAllScores
        if (mg_match(hm->uri, mg_str("/getAllScores"), NULL)) {
            char* scores = get_all_scores();
            if (scores) {
                mg_http_reply(c, 200, "Content-Type: text/plain\r\n", scores);
                free(scores);
            } else {
                mg_http_reply(c, 500, "Content-Type: text/plain\r\n", "");
            }
            return;
        }

        // 404 for unknown routes
        mg_http_reply(c, 404, "Content-Type: text/plain\r\n", "Not Found");
    }
}
