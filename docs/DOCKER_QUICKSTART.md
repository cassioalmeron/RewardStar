# Docker Quick Start Guide

## Running the Application

### Start Containers
```bash
docker-compose up -d
```

### Stop Containers
```bash
docker-compose down
```

## Access Points

| Service | URL | Purpose |
|---------|-----|---------|
| Frontend | http://localhost:3000 | RewardStar web application |
| Backend API | http://localhost:5000 | REST API endpoints |
| Swagger UI | http://localhost:5000/swagger/index.html | API documentation |

## Useful Commands

### View Status
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

### Rebuild Images
```bash
# Rebuild all
docker-compose build --no-cache

# Rebuild specific service
docker-compose build --no-cache backend
docker-compose build --no-cache frontend
```

### Execute Commands in Container
```bash
# Backend
docker-compose exec backend bash

# Frontend
docker-compose exec frontend sh
```

### View Database
```bash
# SQLite database is stored at Backend/data/
ls -la Backend/data/
```

## Troubleshooting

### Port Already in Use
If port 3000 or 5000 is in use, edit `docker-compose.yml`:
```yaml
frontend:
  ports:
    - "8000:80"  # Change 8000 to any available port
```

### Backend Won't Start
Check logs:
```bash
docker-compose logs backend
```

### Frontend Can't Connect to Backend
The frontend proxy is configured in `Frontend/nginx.conf` to connect to `http://backend:5000`. This only works within the Docker network.

## Image Sizes

- **rewardstar-backend**: ~300MB (ASP.NET Core Runtime)
- **rewardstar-frontend**: ~20MB (Nginx + built React app)

## Files Created

- `Backend/Dockerfile` - Multi-stage build for .NET API
- `Frontend/Dockerfile` - Multi-stage build for React + Nginx
- `Frontend/nginx.conf` - Nginx configuration with API proxy
- `docker-compose.yml` - Container orchestration
- `Backend/.dockerignore` - Exclude unnecessary files
- `Frontend/.dockerignore` - Exclude unnecessary files
- `DOCKER.md` - Detailed documentation
- `DOCKER_QUICKSTART.md` - This file
