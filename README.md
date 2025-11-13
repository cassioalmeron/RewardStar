# RewardStar

A full-stack application for managing and tracking rewards activities. Built with ASP.NET Core 8.0 backend and React 19 frontend.

## Project Structure

```
RewardStar/
├── Backend/                 # ASP.NET Core 8.0 API
│   ├── RewardStar.Api/     # API controllers and configuration
│   ├── RewardStart.Core/   # Data models and database context
│   ├── RewardStar.Console/ # Console application
│   ├── Dockerfile          # Docker build configuration for backend
│   └── entrypoint.sh       # Container startup script
├── Frontend/               # React 19 + Vite application
│   ├── src/               # React components and pages
│   ├── public/            # Static assets
│   ├── Dockerfile         # Docker build configuration for frontend
│   ├── nginx.conf         # Nginx reverse proxy configuration
│   └── vite.config.ts     # Vite build configuration
├── docker-compose.yml     # Multi-container orchestration
├── .dockerignore          # Files to exclude from Docker build context
└── README.md              # This file
```

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database**: SQLite with Entity Framework Core
- **API**: RESTful API with Swagger documentation
- **Containerization**: Docker

### Frontend
- **Framework**: React 19
- **Build Tool**: Vite
- **Package Manager**: npm
- **Testing**: Jest
- **Server**: Nginx (in Docker)

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Node.js 20+
- npm or yarn
- Docker & Docker Compose (for containerized deployment)

### Local Development

#### Backend Setup

```bash
cd Backend
dotnet restore
dotnet build
dotnet run --project RewardStar.Api/RewardStar.Api.csproj
```

The API will be available at `http://localhost:5000` and Swagger UI at `http://localhost:5000/swagger`

#### Frontend Setup

```bash
cd Frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173`

### Docker Deployment

#### Prerequisites
- Docker 20.10+
- Docker Compose 2.0+

#### Quick Start

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

**Access Points:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

#### Services

| Service | Port | Technology | Description |
|---------|------|-----------|-------------|
| **Frontend** | 3000 | React 19 + Nginx | Web application interface |
| **Backend** | 5000 | ASP.NET Core 8.0 | REST API server |

#### Database

- **Type**: SQLite
- **Location**: `./Backend/data/RewardStar.db`
- **Auto-Migration**: Enabled - migrations run automatically on container startup

#### Volumes

- `./Backend/data:/app/data` - Persistent SQLite database storage

#### Health Checks

The backend service includes health checks that verify API availability every 30 seconds.

### Available Scripts

#### Backend

```bash
# Run in development mode
dotnet run --project Backend/RewardStar.Api/RewardStar.Api.csproj

# Build release version
dotnet build -c Release

# Run tests
dotnet test

# Run migrations
dotnet ef database update --project Backend/RewardStart.Core/
```

#### Frontend

```bash
# Development server with hot reload
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run tests
npm test

# Lint code
npm run lint
```

## API Documentation

The backend API is documented using Swagger/OpenAPI. When running locally or in Docker:

- **Swagger UI**: http://localhost:5000/swagger
- **OpenAPI JSON**: http://localhost:5000/swagger/v1/swagger.json

## Architecture

### Backend Architecture

- **Controllers**: Handle HTTP requests and responses
- **Models**: Define data structures for activities and rewards
- **DbContext**: Entity Framework Core context for database operations
- **Migrations**: Database schema version control

### Frontend Architecture

- **Pages**: Main application pages (Activities, Parameters, etc.)
- **Components**: Reusable React components
- **Services**: API communication and business logic
- **Tests**: Unit and integration tests with Jest

### Communication

- Frontend communicates with backend via REST API
- CORS is configured to allow frontend-to-backend communication
- Nginx reverse proxy handles API routing in Docker environment

## Environment Variables

### Backend (Docker)

- `ASPNETCORE_ENVIRONMENT`: Application environment (Development, Production)
- `ASPNETCORE_URLS`: Server URL and port (default: http://+:5000)
- `DB_PATH`: SQLite database file path (default: /app/data/RewardStar.db)

### Frontend (Docker)

- Configured via `nginx.conf`
- API proxy: `http://backend:5000`

## Database

### Entity Framework Core Setup

The application uses Entity Framework Core 8.0 with SQLite:

1. **Database Context**: `RewardStartDbContext` in `Backend/RewardStart.Core/RewardStartDbContext.cs`
2. **Configuration**: Entity configurations in `Backend/RewardStart.Core/` with `IEntityTypeConfiguration`
3. **Migrations**: Stored in `Backend/RewardStart.Core/Migrations/`

### Running Migrations

**Locally:**
```bash
cd Backend/RewardStart.Core
dotnet ef database update
```

**In Docker:**
Migrations run automatically when the backend container starts.

## Troubleshooting

### Docker Issues

#### Container fails to start
```bash
# Check logs
docker-compose logs backend

# Verify database directory exists
docker exec rewardstar-backend ls -la /app/data
```

#### Database is locked
```bash
# Restart backend container
docker-compose restart backend
```

#### Port already in use
```bash
# Change ports in docker-compose.yml
# Or kill the process using the port
```

### Frontend Issues

#### Hot reload not working
- Ensure `npm run dev` is running in watch mode
- Check network connectivity between containers

#### API calls failing
- Verify backend is running: `curl http://localhost:5000/health`
- Check Nginx proxy configuration in `Frontend/nginx.conf`

### Backend Issues

#### Database migration errors
```bash
# Reset migrations (development only)
dotnet ef database drop
dotnet ef database update
```

#### Swagger not loading
- Verify backend is running on correct port
- Check ASPNETCORE_URLS environment variable

## Production Considerations

### Security
- HTTPS should be configured in production (use load balancer or reverse proxy)
- Environment variables should be managed securely
- Database backups should be implemented

### Performance
- Consider implementing caching strategies
- Monitor database query performance
- Implement API rate limiting

### Scaling
- Backend: Can be scaled horizontally behind a load balancer
- Database: Consider migrating to PostgreSQL for better concurrency
- Frontend: Static files can be served by CDN

## Contributing

1. Create a feature branch
2. Make your changes
3. Test locally
4. Submit a pull request

## License

Specify your project's license here.

## Support

For issues, questions, or contributions, please open an issue in the repository.

---

**Last Updated**: November 2024
