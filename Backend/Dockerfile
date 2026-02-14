# Multi-stage build for ASP.NET Core API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy solution and project files
COPY ["RewardStar.sln", "."]
COPY ["RewardStar.Core/RewardStar.Core.csproj", "RewardStar.Core/"]
COPY ["RewardStar.Api/RewardStar.Api.csproj", "RewardStar.Api/"]
COPY ["RewardStar.Console/RewardStar.Console.csproj", "RewardStar.Console/"]
COPY ["RewardStar.Tests/RewardStar.Tests.csproj", "RewardStar.Tests/"]

# Restore dependencies
RUN dotnet restore "RewardStar.sln"

# Copy source code
COPY . .

# Build the API project
RUN dotnet build "RewardStar.sln" -c Release -o /app/build

# Publish the API
FROM build AS publish
RUN dotnet publish "RewardStar.Api/RewardStar.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install curl for health checks and clean apt cache to minimize image size
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create database directory for SQLite with proper permissions
RUN mkdir -p /app/data && chmod 755 /app/data

# ARG for configurable port (passed from docker-compose.yml)
ARG API_PORT=5002

# Expose port using ARG
EXPOSE ${API_PORT}

# Set environment variables (will be overridden by docker-compose.yml)
ENV ASPNETCORE_URLS=http://+:${API_PORT}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DB_PATH=/app/data/RewardStar.db

# Health check using ARG
HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=40s \
    CMD curl -f http://localhost:${API_PORT}/health || exit 1

ENTRYPOINT ["sh", "-c", "exec dotnet RewardStar.Api.dll"]
