using Bwadl.Domain.Entities;

namespace Bwadl.Domain.Events;

public record UserCreatedEvent(User User, DateTime CreatedAt) : IDomainEvent;

public interface IDomainEvent
{
    DateTime CreatedAt { get; }
}
