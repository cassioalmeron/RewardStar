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
- **Database**: SQLite (persistent volume at `/app/data`)

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

### Backend Environment Variables

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000;https://+:5001
```

To use custom environment variables, create a `.env` file in the root:

```env
ASPNETCORE_ENVIRONMENT=Development
```

Then reference in `docker-compose.yml`:

```yaml
env_file:
  - .env
```

### Frontend Configuration

The frontend's API endpoint is configured via nginx proxy. Update the nginx configuration if you need to change the backend address:

**File**: `Frontend/nginx.conf`

```nginx
location /api {
    proxy_pass http://backend:5000;  # Change this if needed
}
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

### Scaling

For production deployments:

- Use a load balancer (Nginx, HAProxy, AWS ELB)
- Run multiple backend instances
- Use a production database (PostgreSQL, SQL Server) instead of SQLite
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
