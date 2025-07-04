# CancellationToken Usage Guidelines

## Overview

`CancellationToken` is a critical component for building responsive and resilient applications. It allows operations to be cancelled gracefully when requests are aborted, timeouts occur, or the application is shutting down.

## Layer-by-Layer Guidelines

### 🌐 **API Controller Layer** - ✅ **REQUIRED**

**Purpose**: Accept cancellation from HTTP requests and propagate to application layer.

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers(CancellationToken cancellationToken)
{
    var query = new GetAllUsersQuery();
    var users = await _mediator.Send(query, cancellationToken);
    return Ok(users);
}

[HttpPost]
public async Task<ActionResult<UserResponse>> CreateUser(
    [FromBody] CreateUserRequest request, 
    CancellationToken cancellationToken)
{
    var command = new CreateUserCommand(request.Name, request.Email, request.Type);
    var user = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}
```

**Best Practices**:
- Always add `CancellationToken cancellationToken` as the last parameter
- ASP.NET Core automatically binds the request's cancellation token
- Pass it to all `_mediator.Send()` calls
- Don't manually create cancellation tokens in controllers

### 🏗️ **Application Layer** - ✅ **REQUIRED**

**Purpose**: Handle business logic cancellation and propagate to infrastructure.

#### Command/Query Handlers

```csharp
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        return _mapper.Map<UserDto>(user);
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check business rules
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new DuplicateEmailException(request.Email);
        }
        
        var user = User.Create(request.Name, request.Email, request.Type);
        await _userRepository.AddAsync(user, cancellationToken);
        
        return _mapper.Map<UserDto>(user);
    }
}
```

#### Pipeline Behaviors

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        // Handle validation results...
        
        return await next();
    }
}
```

**Best Practices**:
- All `Handle` methods must accept `CancellationToken`
- Pass it to all repository and external service calls
- Use in FluentValidation: `ValidateAsync(context, cancellationToken)`
- Behaviors should propagate cancellation through the pipeline

### 🏗️ **Infrastructure Layer** - ✅ **REQUIRED**

**Purpose**: Handle I/O operations, database calls, and external services.

#### Repository Interfaces

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
```

#### Repository Implementations

```csharp
public class UserRepository : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
    
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }
}
```

#### External Services

```csharp
public class EmailService : IEmailService
{
    public async Task SendEmailAsync(
        string to, 
        string subject, 
        string body, 
        CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var response = await client.PostAsync(apiUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
```

**Best Practices**:
- Use `= default` for optional parameters in interfaces
- Pass to all Entity Framework operations
- Use with `HttpClient` calls
- Include in all async I/O operations

### 🧠 **Domain Layer** - ❌ **AVOID**

**Purpose**: Keep domain logic pure and infrastructure-agnostic.

```csharp
// ✅ Good - No infrastructure concerns
public class User : Entity<Guid>
{
    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserProfileUpdatedEvent(Id, firstName, lastName));
    }
    
    public bool CanBeDeleted()
    {
        return IsActive && !HasActiveSubscriptions;
    }
}

// ❌ Bad - Domain shouldn't know about cancellation
public async Task<bool> CanBeDeletedAsync(CancellationToken cancellationToken)
{
    // Domain entities should not have async operations
}
```

**Best Practices**:
- Domain entities should be synchronous
- No infrastructure dependencies
- Pure business logic only
- Use domain events for side effects

## Middleware and Exception Handling

### Exception Handling Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Don't log cancellation as an error - it's expected behavior
            _logger.LogDebug("Request was cancelled");
            throw; // Let it bubble up naturally
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

### Security Headers Middleware

```csharp
public class SecurityHeadersMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        
        await _next(context);
    }
}
```

## Common Patterns and Best Practices

### 1. **Always Default Parameter**

```csharp
// ✅ Good - Optional with default
public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)

// ❌ Bad - Required parameter in infrastructure
public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken)
```

### 2. **Parameter Order**

```csharp
// ✅ Good - CancellationToken last
public async Task<User> UpdateUserAsync(Guid id, string name, string email, CancellationToken cancellationToken = default)

// ❌ Bad - CancellationToken not last
public async Task<User> UpdateUserAsync(Guid id, CancellationToken cancellationToken, string name, string email)
```

### 3. **Propagation Chain**

```csharp
// Controller → Application → Infrastructure
[HttpGet("{id}")]
public async Task<ActionResult<User>> GetUser(Guid id, CancellationToken cancellationToken)
    => Ok(await _mediator.Send(new GetUserQuery(id), cancellationToken));

// Handler
public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    => _mapper.Map<UserDto>(await _repository.GetByIdAsync(request.Id, cancellationToken));

// Repository  
public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
```

### 4. **Testing with CancellationToken**

```csharp
[Fact]
public async Task Handle_ValidRequest_ReturnsUser()
{
    // Arrange
    var query = new GetUserQuery(Guid.NewGuid());
    var cancellationToken = new CancellationToken();
    
    // Act
    var result = await _handler.Handle(query, cancellationToken);
    
    // Assert
    result.Should().NotBeNull();
}

[Fact] 
public async Task Handle_CancelledRequest_ThrowsOperationCanceledException()
{
    // Arrange
    var query = new GetUserQuery(Guid.NewGuid());
    var cts = new CancellationTokenSource();
    cts.Cancel();
    
    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _handler.Handle(query, cts.Token));
}
```

### 5. **Timeout Handling**

```csharp
public async Task<User> GetUserWithTimeoutAsync(Guid id)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    return await _userRepository.GetByIdAsync(id, cts.Token);
}
```

## Benefits of Proper CancellationToken Usage

### 1. **Responsive Applications**
- Requests can be cancelled when users navigate away
- Long-running operations don't hold resources unnecessarily
- Better user experience with faster perceived performance

### 2. **Resource Management**
- Database connections released promptly
- HTTP client requests can be cancelled
- Memory usage optimized

### 3. **Graceful Shutdown**
- Application can shutdown cleanly
- In-flight requests complete or cancel gracefully
- No hanging threads or connections

### 4. **Timeout Handling**
- Operations can be time-bounded
- Prevent runaway queries
- Service level agreement compliance

## Anti-Patterns to Avoid

### ❌ **Don't Block on Async**

```csharp
// ❌ Bad - Blocking async call, can't be cancelled
public User GetUser(Guid id)
{
    return _userRepository.GetByIdAsync(id).Result;
}

// ✅ Good - Async all the way
public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _userRepository.GetByIdAsync(id, cancellationToken);
}
```

### ❌ **Don't Ignore CancellationToken**

```csharp
// ❌ Bad - Accepts but doesn't use
public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _userRepository.GetByIdAsync(id); // Missing cancellationToken
}

// ✅ Good - Properly propagated
public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _userRepository.GetByIdAsync(id, cancellationToken);
}
```

### ❌ **Don't Create Tokens in Business Logic**

```csharp
// ❌ Bad - Creating tokens in business logic
public async Task<User> GetUserAsync(Guid id)
{
    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    return await _userRepository.GetByIdAsync(id, cts.Token);
}

// ✅ Good - Accept from caller
public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _userRepository.GetByIdAsync(id, cancellationToken);
}
```

## Validation Checklist

### Controllers ✅
- [ ] All async action methods accept `CancellationToken cancellationToken`
- [ ] CancellationToken is passed to all `_mediator.Send()` calls
- [ ] Parameter is positioned last in method signature

### Application Handlers ✅  
- [ ] All `Handle` methods accept `CancellationToken cancellationToken`
- [ ] Token is passed to all repository calls
- [ ] Token is passed to all external service calls
- [ ] Pipeline behaviors propagate the token

### Infrastructure ✅
- [ ] All repository interface methods have `CancellationToken cancellationToken = default`
- [ ] All implementations pass token to Entity Framework operations
- [ ] External service calls include cancellation token
- [ ] Cache operations support cancellation

### Domain ❌
- [ ] Domain entities remain synchronous
- [ ] No CancellationToken dependencies in domain logic
- [ ] Business rules don't require async operations

This comprehensive approach ensures your application properly handles request cancellation at every layer, improving responsiveness and resource utilization.
