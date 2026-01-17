# Docker Setup for RewardStar

This document explains how to build and run the RewardStar application using Docker containers.

## Status

✅ **Docker containers successfully built and running!**

- **Frontend**: Running on http://localhost:3000
- **Backend API**: Running on http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger/index.html

## Prerequisites

- Docker (v20.10+)
- Docker Compose (v2.0+)

## Project Structure

```
RewardStar/
├── Backend/
│   ├── Dockerfile          # ASP.NET Core 8.0 API container
│   └── .dockerignore       # Files to exclude from Docker build
├── Frontend/
│   ├── Dockerfile          # Node.js + Nginx container
│   ├── nginx.conf          # Nginx reverse proxy configuration
│   └── .dockerignore       # Files to exclude from Docker build
└── docker-compose.yml      # Orchestration configuration
```

## Services

### Backend Service

- **Image**: ASP.NET Core 8.0 Runtime
- **Container Name**: `rewardstar-backend`
- **Port**: `5000` (HTTP), `5001` (HTTPS)
- **Environment**: Production
- **Database**: SQLite (default) or PostgreSQL (configurable via `DATABASE_URL`)
  - SQLite: persistent volume at `/app/data`
  - PostgreSQL: external database via connection string

**Build Strategy**: Multi-stage build
- Stage 1 (`build`): SDK image for compilation
- Stage 2 (`publish`): Publish release artifacts
- Stage 3 (`runtime`): Minimal ASP.NET Core runtime image

### Frontend Service

- **Image**: Nginx Alpine
- **Container Name**: `rewardstar-frontend`
- **Port**: `80` (HTTP)
- **Build Tool**: Vite with React 19
- **Features**:
  - Production-optimized build
  - Gzip compression
  - Static asset caching (1 year)
  - HTML caching (1 hour)
  - React Router SPA support
  - API proxy to backend (`/api` → `http://backend:5000`)

**Build Strategy**: Multi-stage build
- Stage 1: Node.js Alpine for dependencies and build
- Stage 2: Nginx Alpine for serving built assets

## Quick Start

### Build and Run All Services

```bash
# Build images
docker-compose build

# Start services in background
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Access the Application

- **Frontend**: http://localhost
- **Backend API**: http://localhost:5000
- **API Documentation (Swagger)**: http://localhost:5000/swagger/index.html

## Common Commands

### View Running Containers

```bash
docker-compose ps
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
```

### Stop All Services

```bash
docker-compose down
```

### Stop Services and Remove Volumes

```bash
docker-compose down -v
```

### Rebuild Specific Service

```bash
docker-compose build --no-cache backend
docker-compose build --no-cache frontend
```

### Execute Command in Running Container

```bash
# Backend
docker-compose exec backend bash

# Frontend
docker-compose exec frontend sh
```

## Network Communication

The services communicate using Docker's internal network (`rewardstar-network`):

- Frontend → Backend: `http://backend:5000/api`
- External → Frontend: `http://localhost`
- External → Backend: `http://localhost:5000`

## Data Persistence

### Backend Database

The SQLite database is stored in a volume mounted to `/app/data`:

```yaml
volumes:
  - ./Backend/data:/app/data
```

Data persists even after containers are stopped. To reset the database:

```bash
rm -rf Backend/data
docker-compose up -d
```

## Environment Configuration

RewardStar uses a single `.env` file in the project root for all Docker configuration. This file is required before running the application.

### Required Environment File

Copy the example environment file and configure it:

```bash
# The .env file should already exist in the root directory
# If not, create it with the required variables below
```

### Environment Variables Reference

The `.env` file in the project root contains all configuration for both services:

#### Backend Configuration

```env
# API Port Configuration
API_PORT=5003

# ASP.NET Core Configuration
ASPNETCORE_URLS=http://+:5003
ASPNETCORE_ENVIRONMENT=Production

# Database Configuration
# Option 1: SQLite (default)
DB_PATH=/app/data/RewardStar.db

# Option 2: PostgreSQL (uncomment to use)
# DATABASE_URL=Host=postgres;Port=5432;Database=rewardstar;Username=postgres;Password=yourpassword
```

#### Frontend Configuration

```env
# Frontend API URL (build-time variable)
# This is baked into the frontend build during docker build
# For Docker: Use http://localhost:{API_PORT}/api
# The port should match the BACKEND_HOST_PORT for external access
VITE_API_URL=http://localhost:5003/api

# Application Name (optional)
VITE_APP_NAME=RewardStar
```

#### Docker Port Mapping

```env
# Backend external port (host machine)
BACKEND_HOST_PORT=5003

# Frontend external port (host machine)
FRONTEND_HOST_PORT=83
```

### How Environment Variables Work

1. **Build-time variables** (Frontend):
   - `VITE_API_URL` and `VITE_APP_NAME` are passed as build args during `docker-compose build`
   - These are compiled into the frontend JavaScript bundle
   - The frontend cannot be reconfigured after building without rebuilding

2. **Runtime variables** (Backend):
   - Backend environment variables can be overridden at runtime
   - These are set when the container starts via `docker-compose up`

### Overriding Environment Variables

You can override variables without editing the `.env` file:

```bash
# Override at runtime
VITE_API_URL=http://custom-api:5003/api docker-compose up

# Override for build
VITE_API_URL=http://custom-api:5003/api docker-compose build frontend
```

### Frontend API URL Configuration

The frontend uses `VITE_API_URL` exclusively from the environment variable. This value is:

- Set in the `.env` file
- Passed to the Dockerfile during build via `docker-compose.yml`
- Compiled into the application bundle at build time
- **Cannot be changed after build** without rebuilding the container

To change the API URL:

1. Update `VITE_API_URL` in the `.env` file
2. Rebuild the frontend container:

```bash
docker-compose build frontend
docker-compose up -d frontend
```

### Environment Variable Validation

The Docker build process includes validation:

- If `VITE_API_URL` is not set, the build will use the default: `http://localhost:5002/api`
- Ensure your `.env` file has the correct API URL before building

### Troubleshooting Environment Issues

#### Frontend can't connect to API

1. Check the `VITE_API_URL` in your `.env` file
2. Verify it matches the external access URL (usually `http://localhost:{BACKEND_HOST_PORT}/api`)
3. Rebuild the frontend if you changed the variable:

```bash
docker-compose build --no-cache frontend
docker-compose up -d
```

#### Variables not taking effect

1. Ensure the `.env` file is in the project root (same directory as `docker-compose.yml`)
2. For frontend changes, you must rebuild (build-time variables)
3. For backend changes, restart is sufficient (runtime variables)

```bash
# After changing backend variables
docker-compose restart backend

# After changing frontend variables
docker-compose build frontend
docker-compose up -d frontend
```

## Troubleshooting

### Container Won't Start

Check logs for errors:

```bash
docker-compose logs backend
docker-compose logs frontend
```

### Port Already in Use

If ports 80 or 5000 are already in use:

```bash
# Check what's using the port
lsof -i :80
lsof -i :5000

# Or use docker-compose with different ports
docker-compose down
# Edit ports in docker-compose.yml
docker-compose up -d
```

### Database Connection Issues

Ensure the data directory exists and has correct permissions:

```bash
mkdir -p Backend/data
chmod 755 Backend/data
docker-compose restart backend
```

### Frontend Can't Connect to Backend

Verify the nginx configuration has the correct backend service name:

```bash
# Inside frontend container
docker-compose exec frontend ping backend
```

## Performance Optimization

### Frontend Optimization

The Nginx configuration includes:

- **Gzip Compression**: Reduces bandwidth by 70-80%
- **Cache Control**:
  - Static assets (JS, CSS, images): 1 year cache
  - HTML files: 1 hour cache
- **Multi-stage Build**: Reduces image size from 800MB+ to ~20MB

### Backend Optimization

- **Multi-stage Build**: Reduces image size by 80% by using SDK only for build
- **Health Checks**: Automatic container restart on failure
- **Minimal Runtime**: Uses Alpine-based runtime image

## Production Considerations

### Security

1. **Change CORS Policy**: The backend currently allows all origins. For production:

   ```csharp
   policy
       .WithOrigins("https://yourdomain.com")
       .AllowAnyHeader()
       .AllowAnyMethod();
   ```

2. **Use HTTPS**: Enable SSL/TLS certificates

3. **Environment Secrets**: Use Docker secrets or environment files for sensitive data

### Database Configuration

The application supports both SQLite and PostgreSQL databases:

#### SQLite (Default)

SQLite is used by default and is suitable for development and small deployments:

- No additional setup required
- Data stored in a file (`/app/data/RewardStar.db` in container)
- Persists via Docker volume
- Configuration via `DB_PATH` environment variable

#### PostgreSQL (Production)

For production deployments, PostgreSQL is recommended:

1. **Set the DATABASE_URL environment variable** in your `.env` file:

   ```env
   DATABASE_URL=Host=postgres;Port=5432;Database=rewardstar;Username=postgres;Password=yourpassword
   ```

2. **Add PostgreSQL service to docker-compose.yml** (optional):

   ```yaml
   services:
     postgres:
       image: postgres:16-alpine
       container_name: rewardstar-postgres
       environment:
         - POSTGRES_DB=rewardstar
         - POSTGRES_USER=postgres
         - POSTGRES_PASSWORD=yourpassword
       volumes:
         - postgres-data:/var/lib/postgresql/data
       networks:
         - rewardstar-network
       restart: unless-stopped

     backend:
       depends_on:
         - postgres
       environment:
         - DATABASE_URL=Host=postgres;Port=5432;Database=rewardstar;Username=postgres;Password=yourpassword

   volumes:
     postgres-data:
       driver: local
   ```

3. **Update backend service** to add the DATABASE_URL environment variable

4. **Run migrations** after switching to PostgreSQL:

   ```bash
   docker-compose exec backend dotnet ef database update
   ```

#### Switching Between Databases

The application automatically detects which database to use:

- If `DATABASE_URL` is set → Uses PostgreSQL
- If `DATABASE_URL` is not set → Uses SQLite with `DB_PATH`

### Scaling

For production deployments:

- Use a load balancer (Nginx, HAProxy, AWS ELB)
- Run multiple backend instances
- **Use PostgreSQL for production** (configured via `DATABASE_URL`)
- Implement proper logging and monitoring

## Docker Image Sizes

After building:

```
rewardstar-backend   ~300MB
rewardstar-frontend  ~20MB
```

## CI/CD Integration

For automated builds and deployments, add to your CI/CD pipeline:

```bash
# Build images
docker-compose build

# Push to registry (optional)
docker tag rewardstar-backend:latest myregistry/rewardstar-backend:latest
docker push myregistry/rewardstar-backend:latest

# Deploy
docker-compose -f docker-compose.yml up -d
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [ASP.NET Core on Docker](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)
- [Nginx Documentation](https://nginx.org/en/docs/)
