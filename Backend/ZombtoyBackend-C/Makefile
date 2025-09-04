# ZombtoyBackend-C Makefile

CC = gcc
CFLAGS = -Wall -Wextra -Iinclude -I.
LDFLAGS = -lsqlite3
TARGET = zombtoy_backend_c
SRC_DIR = src
OBJ_DIR = obj

# Source files (exclude mongoose.c since it's compiled separately)
SRCS = $(SRC_DIR)/main.c $(SRC_DIR)/database.c $(SRC_DIR)/http_handlers.c
OBJS = $(SRCS:$(SRC_DIR)/%.c=$(OBJ_DIR)/%.o) mongoose.o

# Default target
all: $(TARGET)

# Link object files to create executable
$(TARGET): $(OBJS)
	$(CC) $(CFLAGS) -o $@ $^ $(LDFLAGS)

# Compile source files to object files
$(OBJ_DIR)/%.o: $(SRC_DIR)/%.c | $(OBJ_DIR)
	$(CC) $(CFLAGS) -c $< -o $@

# Compile mongoose.c
mongoose.o: mongoose.c
	$(CC) $(CFLAGS) -c $< -o $@

# Create obj directory if it doesn't exist
$(OBJ_DIR):
	mkdir -p $(OBJ_DIR)

# Clean build artifacts
clean:
	rm -rf $(OBJ_DIR) $(TARGET) mongoose.o zombtoy_c.db

# Run the server
run: $(TARGET)
	./$(TARGET)

# Test endpoints (requires curl)
test:
	@echo "Testing endpoints..."
	curl -X POST http://localhost:8080/addScore -d "100"
	@echo ""
	curl http://localhost:8080/getAllScores
	@echo ""

.PHONY: all clean run test
