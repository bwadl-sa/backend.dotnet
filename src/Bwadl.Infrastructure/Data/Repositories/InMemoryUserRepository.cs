using Bwadl.Domain.Entities;
using Bwadl.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Bwadl.Infrastructure.Data.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly ILogger<InMemoryUserRepository> _logger;

    public InMemoryUserRepository(ILogger<InMemoryUserRepository> logger)
    {
        _logger = logger;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user by ID: {UserId}", id);
        
        _users.TryGetValue(id, out var user);
        
        if (user != null)
            _logger.LogInformation("User found with ID: {UserId}", id);
        else
            _logger.LogInformation("User not found with ID: {UserId}", id);
            
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user by email: {Email}", email);
        
        var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        
        if (user != null)
            _logger.LogInformation("User found with email: {Email}, ID: {UserId}", email, user.Id);
        else
            _logger.LogInformation("User not found with email: {Email}", email);
            
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all users from repository");
        
        var users = _users.Values.OrderBy(u => u.CreatedAt).AsEnumerable();
        var userList = users.ToList();
        
        _logger.LogInformation("Retrieved {UserCount} users from repository", userList.Count);
        return Task.FromResult(userList.AsEnumerable());
    }

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding user to repository: {UserId}, Name: {Name}, Email: {Email}", 
            user.Id, user.Name, user.Email);
        
        _users.TryAdd(user.Id, user);
        
        _logger.LogInformation("User added successfully to repository: {UserId}", user.Id);
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user in repository: {UserId}, Name: {Name}, Email: {Email}", 
            user.Id, user.Name, user.Email);
        
        _users.TryUpdate(user.Id, user, _users[user.Id]);
        
        _logger.LogInformation("User updated successfully in repository: {UserId}", user.Id);
        return Task.FromResult(user);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user from repository: {UserId}", id);
        
        var removed = _users.TryRemove(id, out _);
        
        if (removed)
            _logger.LogInformation("User deleted successfully from repository: {UserId}", id);
        else
            _logger.LogWarning("Failed to delete user from repository: {UserId}", id);
            
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if user exists in repository: {UserId}", id);
        
        var exists = _users.ContainsKey(id);
        
        _logger.LogInformation("User exists check result for {UserId}: {Exists}", id, exists);
        return Task.FromResult(exists);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if user exists by email in repository: {Email}", email);
        
        var exists = _users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        
        _logger.LogInformation("User exists by email check result for {Email}: {Exists}", email, exists);
        return Task.FromResult(exists);
    }
}
