# Architecture Overview

## Introduction

Bwadl is a comprehensive enterprise .NET Core solution built using Clean Architecture principles, CQRS pattern, and Domain-Driven Design (DDD). The architecture emphasizes separation of concerns, testability, maintainability, and scalability.

## Clean Architecture

### Architecture Layers

The solution follows Uncle Bob's Clean Architecture with four distinct layers:

```
┌─────────────────────────────────────────┐
│            API Layer                    │
│  (Controllers, Middleware, Filters)     │
└─────────────────────────────────────────┘
┌─────────────────────────────────────────┐
│         Application Layer               │
│    (Use Cases, DTOs, Behaviors)        │
└─────────────────────────────────────────┘
┌─────────────────────────────────────────┐
│       Infrastructure Layer             │
│ (Repositories, External Services, DB)   │
└─────────────────────────────────────────┘
┌─────────────────────────────────────────┐
│          Domain Layer                   │
│   (Entities, Value Objects, Events)     │
└─────────────────────────────────────────┘
```

### Layer Dependencies

- **API Layer** → Application Layer
- **Application Layer** → Domain Layer
- **Infrastructure Layer** → Application + Domain Layers
- **Domain Layer** → No dependencies (pure business logic)

## Project Structure

### Bwadl.API
- **Purpose**: Web API layer, entry point for HTTP requests
- **Responsibilities**:
  - HTTP request handling
  - API versioning
  - Swagger documentation
  - Security headers
  - Exception handling middleware
  - Health checks
- **Key Components**:
  - Controllers (V1, V2 versions)
  - Middleware (Exception handling, Security headers)
  - Configuration (API versioning, Swagger, Health checks)
  - Filters and Extensions

### Bwadl.Application
- **Purpose**: Business logic orchestration layer
- **Responsibilities**:
  - Use case implementation
  - CQRS command/query handling
  - Cross-cutting concerns (validation, logging, caching)
  - Data transformation (DTOs)
- **Key Components**:
  - Features (organized by business domain)
  - Common behaviors (Validation, Logging, Caching, Resilience)
  - DTOs and Mappings
  - Validators (FluentValidation)

### Bwadl.Domain
- **Purpose**: Core business domain layer
- **Responsibilities**:
  - Business entities
  - Domain events
  - Business rules and invariants
  - Value objects
- **Key Components**:
  - Entities
  - Value Objects
  - Domain Events
  - Domain Exceptions
  - Business Interfaces

### Bwadl.Infrastructure
- **Purpose**: External service integration layer
- **Responsibilities**:
  - Data persistence
  - External API integration
  - Caching implementation
  - Security services
  - Messaging
- **Key Components**:
  - Data (EF Core, Repositories)
  - Caching (Redis, In-Memory)
  - Security (JWT, API Keys)
  - External Services
  - Messaging (Event Bus)
  - Monitoring (Metrics, Tracing)
  - Resilience (Retry policies, Circuit breakers)

### Bwadl.Shared
- **Purpose**: Shared utilities and configurations
- **Responsibilities**:
  - Common configuration classes
  - Shared constants
  - Utility functions
  - Cross-cutting extensions
- **Key Components**:
  - Configuration options
  - Constants
  - Extensions
  - Utilities

## CQRS Pattern

### Command Query Responsibility Segregation

The application implements CQRS using MediatR to separate read and write operations:

```
Command Flow:
HTTP Request → Controller → Command → Command Handler → Repository → Database

Query Flow:
HTTP Request → Controller → Query → Query Handler → Read Model → Database
```

### Benefits

- **Scalability**: Separate read and write models
- **Performance**: Optimized queries and commands
- **Maintainability**: Clear separation of concerns
- **Flexibility**: Different data models for reads/writes

### Implementation

- **Commands**: Represent business operations that change state
- **Queries**: Represent data retrieval operations
- **Handlers**: Process commands and queries
- **Behaviors**: Cross-cutting concerns pipeline

## Enterprise Features

### Security
- JWT authentication
- API key authorization
- Security headers middleware
- Secret management integration
- Role-based access control

### Observability
- Structured logging with Serilog
- Health checks
- Metrics collection
- Distributed tracing
- Performance monitoring

### Resilience
- Retry policies with Polly
- Circuit breaker pattern
- Timeout handling
- Bulkhead isolation

### Caching
- Distributed caching (Redis)
- In-memory caching
- Cache-aside pattern
- Automatic cache invalidation

### Validation
- Request validation with FluentValidation
- Business rule validation
- Input sanitization
- Error handling

### Testing
- Unit tests for domain logic
- Integration tests for API endpoints
- Test containers for database testing
- Behavior-driven development support

## Technology Stack

### Core Framework
- **.NET 8.0**: Latest LTS version
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for data access

### Libraries and Packages
- **MediatR**: CQRS implementation
- **AutoMapper**: Object mapping
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Polly**: Resilience patterns
- **Swashbuckle**: API documentation

### Infrastructure
- **Redis**: Distributed caching
- **SQL Server**: Primary database
- **Docker**: Containerization
- **Kubernetes**: Orchestration

## Design Principles

### SOLID Principles
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes must be substitutable
- **Interface Segregation**: Clients shouldn't depend on unused interfaces
- **Dependency Inversion**: Depend on abstractions, not concretions

### Domain-Driven Design
- **Ubiquitous Language**: Shared vocabulary between developers and domain experts
- **Bounded Contexts**: Clear boundaries between different parts of the system
- **Aggregates**: Consistency boundaries for business operations
- **Domain Events**: Decoupled communication between aggregates

### Additional Principles
- **Fail Fast**: Validate inputs early
- **Immutability**: Prefer immutable objects where possible
- **Composition over Inheritance**: Favor composition
- **Convention over Configuration**: Sensible defaults
