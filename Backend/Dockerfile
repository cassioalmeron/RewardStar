# Multi-stage build for ASP.NET Core API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy solution and project files
COPY ["RewardStart.sln", "."]
COPY ["RewardStart.Core/RewardStart.Core.csproj", "RewardStart.Core/"]
COPY ["RewardStar.Api/RewardStar.Api.csproj", "RewardStar.Api/"]
COPY ["RewardStar.Console/RewardStar.Console.csproj", "RewardStar.Console/"]

# Restore dependencies
RUN dotnet restore "RewardStart.sln"

# Copy source code
COPY . .

# Build the API project
RUN dotnet build "RewardStart.sln" -c Release -o /app/build

# Publish the API
FROM build AS publish
RUN dotnet publish "RewardStar.Api/RewardStar.Api.csproj" -c Release -o /app/publish

# Copy entrypoint script to publish output
COPY entrypoint.sh /app/publish/entrypoint.sh
RUN chmod +x /app/publish/entrypoint.sh

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create database directory for SQLite
RUN mkdir -p /app/data

# Expose port
EXPOSE 5000

# Set environment variables for HTTP-only in production
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DB_PATH=/app/data/RewardStar.db

ENTRYPOINT ["sh", "-c", "exec dotnet RewardStar.Api.dll"]
