# Getting Started

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** or **Visual Studio Code**
- **Docker Desktop** (for containerization)
- **Git** (for version control)
- **SQL Server** (local or containerized)
- **Redis** (optional, for caching)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/bwadl.git
cd bwadl
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure User Secrets

Set up user secrets for sensitive configuration:

```bash
cd src/Bwadl.API
dotnet user-secrets init
dotnet user-secrets set "Security:Jwt:SecretKey" "your-super-secret-jwt-key-here"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

### 4. Update Configuration

Update `appsettings.Development.json` in the API project:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Security": {
    "Jwt": {
      "Issuer": "Bwadl.API",
      "Audience": "Bwadl.Client",
      "ExpirationMinutes": 60
    },
    "ApiKeys": {
      "RequireApiKey": false,
      "ValidApiKeys": []
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BwadlDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Caching": {
    "Redis": {
      "ConnectionString": "localhost:6379",
      "InstanceName": "Bwadl"
    }
  }
}
```

## Running the Application

### Option 1: Using VS Code Tasks

Open the project in VS Code and use the predefined tasks:

```bash
# Build the solution
Ctrl+Shift+P → Tasks: Run Task → build

# Run in watch mode (recommended for development)
Ctrl+Shift+P → Tasks: Run Task → watch

# Clean the solution
Ctrl+Shift+P → Tasks: Run Task → clean
```

### Option 2: Command Line

```bash
# Build the solution
dotnet build

# Run the API project
cd src/Bwadl.API
dotnet run

# Run in watch mode (auto-reload on changes)
dotnet watch run
```

### Option 3: Docker

```bash
# Build and run with Docker
docker-compose up --build

# Run in background
docker-compose up -d
```

## Development Setup

### Environment Configuration

1. **Development Environment**: Configured for local development with detailed logging
2. **Staging Environment**: Similar to production but with additional debugging
3. **Production Environment**: Optimized for performance and security

### Database Setup

#### Using Entity Framework Migrations

```bash
# Add a new migration
dotnet ef migrations add InitialCreate --project src/Bwadl.Infrastructure --startup-project src/Bwadl.API

# Update the database
dotnet ef database update --project src/Bwadl.Infrastructure --startup-project src/Bwadl.API
```

#### Using Docker for SQL Server

```bash
# Run SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### Redis Setup (Optional)

```bash
# Run Redis in Docker
docker run --name redis -p 6379:6379 -d redis:alpine

# Or use Redis Stack with RedisInsight
docker run -d --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
```

## IDE Configuration

### Visual Studio Code

Install recommended extensions:

- **C# Dev Kit**: Enhanced C# development
- **REST Client**: Test API endpoints
- **Docker**: Container management
- **GitLens**: Enhanced Git capabilities
- **Thunder Client**: API testing (alternative to Postman)

### Visual Studio 2022

Ensure you have the following workloads:
- **ASP.NET and web development**
- **.NET Core cross-platform development**
- **Data storage and processing** (for Entity Framework tools)

## Testing the Setup

### 1. Health Checks

Once the application is running, verify health status:

```
GET https://localhost:7260/health
GET https://localhost:7260/health/ui
```

### 2. Swagger Documentation

Access the API documentation:

```
https://localhost:7260/swagger
```

### 3. API Endpoints

Test basic endpoints using the provided `.http` files:

```bash
# Open Bwadl.API.http in VS Code
# Click "Send Request" on any endpoint
```

### 4. Run Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Bwadl.Tests.Unit
dotnet test tests/Bwadl.Tests.Integration
```

## Project Structure Overview

```
Bwadl/
├── src/
│   ├── Bwadl.API/           # Web API layer
│   ├── Bwadl.Application/   # Business logic layer
│   ├── Bwadl.Domain/        # Domain entities and rules
│   ├── Bwadl.Infrastructure/# Data access and external services
│   └── Bwadl.Shared/        # Shared utilities and configurations
├── tests/
│   ├── Bwadl.Tests.Unit/    # Unit tests
│   └── Bwadl.Tests.Integration/ # Integration tests
├── docs/                    # Documentation
├── infrastructure/          # Deployment scripts and configurations
└── tools/                   # Development tools and scripts
```

## Common Issues and Solutions

### Issue: Port Already in Use

```bash
# Find process using port 5281 or 7260
lsof -ti:5281
lsof -ti:7260

# Kill the process
kill -9 <PID>
```

### Issue: Database Connection Errors

1. Verify SQL Server is running
2. Check connection string in user secrets
3. Ensure database exists (run migrations)

### Issue: Missing NuGet Packages

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

### Issue: SSL Certificate Errors (Development)

```bash
# Trust the development certificate
dotnet dev-certs https --trust
```

## Next Steps

1. **Explore the API**: Use Swagger UI to understand available endpoints
2. **Review the Architecture**: Read the [Architecture Overview](architecture.md)
3. **Understand Development Guidelines**: Follow [Development Guidelines](development-guidelines.md)
4. **Set Up CI/CD**: Configure deployment using [Deployment Guide](deployment.md)
5. **Write Your First Feature**: Follow CQRS patterns in the existing codebase

## Useful Commands

```bash
# Watch for file changes and auto-restart
dotnet watch run --project src/Bwadl.API

# Run with specific environment
dotnet run --environment Staging

# Generate API client
dotnet tool install -g NSwag.ConsoleCore
nswag run

# Entity Framework commands
dotnet ef migrations list
dotnet ef database drop
dotnet ef database update

# Package management
dotnet add package <PackageName>
dotnet remove package <PackageName>
dotnet list package
```

## Getting Help

- **Documentation**: Check the `docs/` folder for detailed guides
- **Issues**: Report bugs and request features on the project repository
- **Code Examples**: Review existing features for implementation patterns
- **Architecture Questions**: Refer to the architecture documentation and CQRS examples
