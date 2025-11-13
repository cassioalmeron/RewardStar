#!/bin/bash
set -e

echo "=========================================="
echo "RewardStar API - Docker Entrypoint"
echo "=========================================="

# Export environment variable for db path
export DB_PATH=/app/data/RewardStar.db

# Ensure data directory exists
mkdir -p /app/data
chmod 755 /app/data

echo ""
echo "Starting application..."
echo "Database path: $DB_PATH"
echo ""

# Start the application
# EF Core will run migrations automatically when DbContext initializes
exec dotnet RewardStar.Api.dll
