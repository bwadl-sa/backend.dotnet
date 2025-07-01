using Bwadl.Domain.ValueObjects;

namespace Bwadl.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public UserType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public User(string name, string email, UserType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateType(UserType type)
    {
        Type = type;
        UpdatedAt = DateTime.UtcNow;
    }

}