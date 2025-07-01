# Bwadl Documentation

## Table of Contents

- [Architecture Overview](architecture.md)
- [Getting Started](getting-started.md)
- [API Reference](api-reference.md)
- [Development Guidelines](development-guidelines.md)
- [Deployment Guide](deployment.md)

## Quick Links

- [Clean Architecture Principles](architecture.md#clean-architecture)
- [CQRS Implementation](architecture.md#cqrs-pattern)
- [Logging Strategy](development-guidelines.md#logging)
- [Testing Approach](development-guidelines.md#testing)

## Architecture Diagrams

### Clean Architecture Layers
```
API Layer (Controllers, Middleware)
    ↓
Application Layer (Use Cases, DTOs)
    ↓
Infrastructure Layer (Repositories, Services)
    ↓
Domain Layer (Entities, Business Rules)
```

### Request Flow
```
HTTP Request → Controller → MediatR → Handler → Repository → Database
                   ↓
              Behaviors: Logging, Validation, Caching, Resilience
```

## Key Features

- Clean Architecture implementation
- CQRS with MediatR
- Comprehensive logging with Serilog
- Automatic validation with FluentValidation
- Caching and resilience patterns
- Enterprise-grade error handling
- Complete test coverage
