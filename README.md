# Bwadl - Enterprise .NET Clean Architecture Solution

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/yourusername/bwadl)

A comprehensive enterprise-grade .NET solution demonstrating **Clean Architecture (Onion Architecture)** principles with modern patterns and practices.

## 🏗️ Architecture Overview

This solution implements Clean Architecture with clear separation of concerns across multiple layers:

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│  Controllers, Middleware, Configuration, Models            │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                   Application Layer                         │
│  Use Cases, DTOs, Validators, Behaviors, Interfaces        │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  Repositories, External Services, Caching, Messaging       │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                    Domain Layer                             │
│  Entities, Value Objects, Domain Events, Business Rules    │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Features

### Core Architecture
- ✅ **Clean Architecture** (Onion Architecture) implementation
- ✅ **CQRS Pattern** with MediatR
- ✅ **Repository Pattern** with interfaces
- ✅ **Dependency Injection** with modular service registration
- ✅ **Domain-Driven Design** principles

### Enterprise Patterns
- ✅ **Pipeline Behaviors** (Logging, Validation, Caching, Resilience)
- ✅ **Global Exception Handling** with middleware
- ✅ **Request/Response Pattern** with DTOs
- ✅ **Specification Pattern** for complex queries
- ✅ **Unit of Work Pattern** for data consistency

### Technology Stack
- ✅ **.NET 8** - Latest .NET framework
- ✅ **MediatR** - Request/response pattern and behaviors
- ✅ **AutoMapper** - Object-to-object mapping
- ✅ **FluentValidation** - Input validation
- ✅ **Entity Framework Core** - Data access (In-Memory for demo)
- ✅ **Serilog** - Structured logging
- ✅ **Polly** - Resilience and fault tolerance
- ✅ **Redis** - Distributed caching (Memory cache for demo)
- ✅ **RabbitMQ** - Message queuing (Mock implementation)
- ✅ **OpenTelemetry** - Observability and monitoring
- ✅ **xUnit** - Unit and integration testing
- ✅ **FluentAssertions** - Test assertions
- ✅ **Testcontainers** - Integration testing with containers

## 🏭 Project Structure

```
Bwadl/
├── src/
│   ├── Bwadl.API/                 # Web API layer
│   │   ├── Controllers/           # API controllers
│   │   ├── Middleware/            # Custom middleware
│   │   ├── Configuration/         # API configuration
│   │   └── Models/               # Request/Response models
│   ├── Bwadl.Application/        # Application services layer
│   │   ├── Features/             # Use cases (Commands/Queries)
│   │   ├── Common/               # Shared application logic
│   │   │   ├── Behaviors/        # MediatR pipeline behaviors
│   │   │   ├── Interfaces/       # Application interfaces
│   │   │   ├── DTOs/            # Data transfer objects
│   │   │   └── Mappings/        # AutoMapper profiles
│   │   └── Validators/           # FluentValidation validators
│   ├── Bwadl.Domain/            # Domain entities and business logic
│   │   ├── Entities/            # Domain entities
│   │   ├── ValueObjects/        # Value objects
│   │   ├── Events/              # Domain events
│   │   ├── Interfaces/          # Domain interfaces
│   │   └── Exceptions/          # Domain exceptions
│   ├── Bwadl.Infrastructure/     # Infrastructure services
│   │   ├── Data/                # Data access implementations
│   │   ├── Caching/             # Caching implementations
│   │   ├── Messaging/           # Message bus implementations
│   │   ├── ExternalServices/    # External API integrations
│   │   └── Security/            # Security implementations
│   └── Bwadl.Shared/            # Shared utilities
├── tests/
│   ├── Bwadl.Tests.Unit/        # Unit tests
│   └── Bwadl.Tests.Integration/ # Integration tests
├── docs/                        # Documentation
├── infrastructure/              # Deployment and infrastructure
└── tools/                       # Development tools
```

## 🛠️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/bwadl.git
   cd bwadl
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/Bwadl.API
   ```

6. **Access the API**
   - API: `https://localhost:5001` or `http://localhost:5000`
   - Swagger UI: `https://localhost:5001/swagger`

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test tests/Bwadl.Tests.Unit
```

### Run Integration Tests Only
```bash
dotnet test tests/Bwadl.Tests.Integration
```

### Test Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📚 API Documentation

The API includes comprehensive Swagger documentation available at `/swagger` when running in development mode.

### Sample Endpoints

#### Users Management
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

#### Sample Request
```json
POST /api/users
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "userType": 1
}
```

## 🔍 Logging and Monitoring

### Structured Logging with Serilog
- Console and file logging configured
- Structured logs with correlation IDs
- Request/response logging through pipeline behaviors
- Performance metrics and timing

### Sample Log Output
```
[2025-07-02 10:30:15.123 INF] Starting request CreateUserCommand
[2025-07-02 10:30:15.124 INF] Found 1 validators for CreateUserCommand
[2025-07-02 10:30:15.125 INF] Creating user with email john.doe@example.com
[2025-07-02 10:30:15.130 INF] User created successfully
[2025-07-02 10:30:15.131 INF] Completed request CreateUserCommand in 8ms
```

## 🏗️ Design Patterns Implemented

### Behavioral Patterns
- **Command Pattern** - Encapsulating requests as objects
- **Observer Pattern** - Domain events and notifications
- **Pipeline Pattern** - Request processing pipeline with behaviors

### Structural Patterns
- **Repository Pattern** - Data access abstraction
- **Adapter Pattern** - External service integrations
- **Facade Pattern** - Simplified interfaces for complex subsystems

### Creational Patterns
- **Dependency Injection** - IoC container for loose coupling
- **Factory Pattern** - Object creation abstraction

## 🔧 Configuration

### Application Settings
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BwadlDb;Trusted_Connection=true"
  }
}
```

## 🚀 Deployment

### Docker Support (Coming Soon)
```dockerfile
# Dockerfile example
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
```

### CI/CD Pipeline (Coming Soon)
- GitHub Actions workflow
- Automated testing
- Code quality checks
- Security scanning

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Standards
- Follow C# coding conventions
- Maintain test coverage above 80%
- Add XML documentation for public APIs
- Follow Clean Architecture principles

## 📋 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- Create an [Issue](https://github.com/yourusername/bwadl/issues)
- Check the [Documentation](docs/)
- Review the [Wiki](https://github.com/yourusername/bwadl/wiki)

## 🙏 Acknowledgments

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Enterprise Application Patterns](https://martinfowler.com/eaaCatalog/)

---

⭐ **Star this repository** if you find it helpful!
