# Development Guidelines

## Introduction

This document outlines the development standards, practices, and guidelines for the Bwadl project. Following these guidelines ensures code consistency, maintainability, and high quality across the entire codebase.

## Code Style and Standards

### C# Coding Conventions

#### Naming Conventions

- **Classes**: PascalCase (`UserService`, `OrderController`)
- **Methods**: PascalCase (`GetUserById`, `ProcessOrder`)
- **Properties**: PascalCase (`FirstName`, `IsActive`)
- **Fields**: camelCase with underscore prefix (`_userRepository`, `_logger`)
- **Parameters**: camelCase (`userId`, `orderDate`)
- **Local Variables**: camelCase (`userCount`, `isValid`)
- **Constants**: PascalCase (`MaxRetryAttempts`, `DefaultTimeout`)

#### File Organization

```csharp
// 1. Using statements (grouped and sorted)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bwadl.Domain.Entities;

// 2. Namespace
namespace Bwadl.Application.Features.Users;

// 3. Class with proper access modifiers
public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    // 4. Private readonly fields
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    // 5. Constructor
    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // 6. Public methods
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Implementation
    }

    // 7. Private methods
    private void ValidateRequest(GetUserByIdQuery request)
    {
        // Implementation
    }
}
```

### Code Formatting

Use the provided `.editorconfig` file for consistent formatting:

```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
trim_trailing_whitespace = true
insert_final_newline = true
indent_style = space
indent_size = 4

[*.{cs,csx,vb,vbx}]
indent_size = 4

[*.{json,js,ts,html,css}]
indent_size = 2
```

## Architecture Patterns

### CQRS Implementation

#### Commands

Commands represent operations that change system state:

```csharp
// Command
public sealed record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password) : IRequest<Guid>;

// Command Handler
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user with email: {Email}", request.Email);

        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        
        var user = User.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            hashedPassword);

        await _userRepository.AddAsync(user, cancellationToken);
        
        _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
        
        return user.Id;
    }
}
```

#### Queries

Queries represent read operations:

```csharp
// Query
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;

// Query Handler
public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"user_{request.UserId}";
        
        if (_cache.TryGetValue(cacheKey, out UserDto? cachedUser))
        {
            _logger.LogDebug("User found in cache: {UserId}", request.UserId);
            return cachedUser!;
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            throw new NotFoundException($"User with ID {request.UserId} was not found.");
        }

        var userDto = _mapper.Map<UserDto>(user);
        
        _cache.Set(cacheKey, userDto, TimeSpan.FromMinutes(15));
        
        return userDto;
    }
}
```

### Domain-Driven Design

#### Entities

```csharp
public sealed class User : Entity<Guid>
{
    private User(
        Guid id,
        string email,
        string firstName,
        string lastName,
        string passwordHash) : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static User Create(
        string email,
        string firstName,
        string lastName,
        string passwordHash)
    {
        // Business rule validation
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be empty.");

        return new User(Guid.NewGuid(), email, firstName, lastName, passwordHash);
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
        
        // Raise domain event
        AddDomainEvent(new UserProfileUpdatedEvent(Id, firstName, lastName));
    }
}
```

#### Value Objects

```csharp
public sealed record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format.", nameof(value));

        Value = value.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);
    }

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);
}
```

## Logging

### Logging Strategy

Use structured logging with Serilog throughout the application:

```csharp
public sealed class UserService
{
    private readonly ILogger<UserService> _logger;

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        using var scope = _logger.BeginScope("Creating user {Email}", request.Email);
        
        _logger.LogInformation("Starting user creation process");

        try
        {
            // Business logic
            var user = await ProcessUserCreation(request);
            
            _logger.LogInformation(
                "User created successfully with ID: {UserId} and Email: {Email}",
                user.Id,
                user.Email);

            return user;
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation failed for user creation: {Email}. Errors: {@ValidationErrors}",
                request.Email,
                ex.Errors);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error occurred while creating user: {Email}",
                request.Email);
            throw;
        }
    }
}
```

### Log Levels

- **Trace**: Very detailed information for debugging
- **Debug**: Information useful for debugging
- **Information**: General application flow
- **Warning**: Unexpected situations that don't stop execution
- **Error**: Errors and exceptions
- **Critical**: Critical errors that require immediate attention

### Sensitive Data

Never log sensitive information:

```csharp
// ❌ Bad - logs sensitive data
_logger.LogInformation("User login attempt: {Email} with password {Password}", 
    email, password);

// ✅ Good - logs safely
_logger.LogInformation("User login attempt: {Email}", email);
```

## Testing

### Testing Strategy

#### Unit Tests

Test individual components in isolation:

```csharp
public sealed class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();
        
        _handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUserId()
    {
        // Arrange
        var command = new CreateUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "password123");

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns("hashed-password");

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

#### Integration Tests

Test the complete request flow:

```csharp
public sealed class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(content);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Email.Should().Be(request.Email);
    }
}
```

### Test Organization

```
tests/
├── Bwadl.Tests.Unit/
│   ├── Application/
│   │   ├── Features/
│   │   │   └── Users/
│   │   │       ├── CreateUserCommandHandlerTests.cs
│   │   │       └── GetUserByIdQueryHandlerTests.cs
│   │   └── Behaviors/
│   ├── Domain/
│   │   └── Entities/
│   │       └── UserTests.cs
│   └── Infrastructure/
└── Bwadl.Tests.Integration/
    ├── Controllers/
    │   └── UsersControllerTests.cs
    └── Health/
        └── HealthCheckTests.cs
```

## Validation

### Input Validation with FluentValidation

```csharp
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z\s]*$")
            .WithMessage("First name can only contain letters and spaces.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z\s]*$")
            .WithMessage("Last name can only contain letters and spaces.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.");
    }
}
```

## Error Handling

### Custom Exceptions

```csharp
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}

public sealed class ValidationException : DomainException
{
    public IReadOnlyCollection<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList().AsReadOnly();
    }
}
```

### Global Exception Handler

```csharp
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            NotFoundException => new ApiResponse<object>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = "NOT_FOUND",
                    Message = exception.Message
                }
            },
            ValidationException validationEx => new ApiResponse<object>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = "One or more validation errors occurred.",
                    Details = validationEx.Errors.Select(e => new ErrorDetail
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    }).ToList()
                }
            },
            _ => new ApiResponse<object>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred."
                }
            }
        };

        context.Response.StatusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
```

## Performance Guidelines

### Async/Await Best Practices

```csharp
// ✅ Good - ConfigureAwait(false) in library code
public async Task<User> GetUserAsync(Guid id)
{
    var user = await _repository.GetByIdAsync(id).ConfigureAwait(false);
    return user;
}

// ✅ Good - Use async all the way down
public async Task<IActionResult> GetUser(Guid id)
{
    var user = await _userService.GetUserAsync(id);
    return Ok(user);
}

// ❌ Bad - Blocking async calls
public User GetUser(Guid id)
{
    return _userService.GetUserAsync(id).Result; // Can cause deadlocks
}
```

### Database Access

```csharp
// ✅ Good - Use projections for DTOs
public async Task<IEnumerable<UserDto>> GetUsersAsync()
{
    return await _context.Users
        .Where(u => u.IsActive)
        .Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            FullName = $"{u.FirstName} {u.LastName}"
        })
        .ToListAsync();
}

// ❌ Bad - Loading full entities when only DTOs needed
public async Task<IEnumerable<UserDto>> GetUsersAsync()
{
    var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
    return _mapper.Map<IEnumerable<UserDto>>(users);
}
```

## Security Guidelines

### Input Sanitization

```csharp
public sealed class UserController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        // Input validation is handled by FluentValidation behaviors
        // Additional sanitization can be done here if needed
        
        var userId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUser), new { id = userId }, new { id = userId });
    }
}
```

### Secret Management

```csharp
// ✅ Good - Use configuration and secret management
public class JwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(User user)
    {
        // Use _options.SecretKey from configuration/secrets
    }
}

// ❌ Bad - Hardcoded secrets
public string GenerateToken(User user)
{
    var secretKey = "hardcoded-secret-key"; // Never do this
}
```

## Git Workflow

### Branch Naming

- `feature/feature-name` - New features
- `bugfix/issue-description` - Bug fixes
- `hotfix/critical-issue` - Critical production fixes
- `chore/task-description` - Maintenance tasks

### Commit Messages

Follow conventional commit format:

```
type(scope): description

[optional body]

[optional footer]
```

Examples:
```
feat(users): add user profile update functionality
fix(auth): resolve JWT token expiration issue
docs(api): update endpoint documentation
test(users): add integration tests for user creation
```

### Pull Request Guidelines

1. **Title**: Clear, descriptive title
2. **Description**: Explain what and why
3. **Testing**: Describe how the change was tested
4. **Breaking Changes**: Highlight any breaking changes
5. **Screenshots**: Include UI changes if applicable

## Code Review Checklist

### Functionality
- [ ] Code solves the intended problem
- [ ] Edge cases are handled
- [ ] Error handling is appropriate
- [ ] Performance is acceptable

### Code Quality
- [ ] Code follows project conventions
- [ ] Methods are focused and not too long
- [ ] Variable names are meaningful
- [ ] Comments explain "why", not "what"

### Testing
- [ ] Unit tests are included and pass
- [ ] Integration tests are included where appropriate
- [ ] Test coverage is adequate
- [ ] Tests are meaningful and not just for coverage

### Security
- [ ] Input validation is present
- [ ] No sensitive data in logs
- [ ] Authentication/authorization is correct
- [ ] SQL injection vulnerabilities are prevented

### Documentation
- [ ] Code is self-documenting
- [ ] Complex logic is commented
- [ ] API documentation is updated
- [ ] README is updated if needed
