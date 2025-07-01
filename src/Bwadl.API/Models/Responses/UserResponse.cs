using Bwadl.Domain.ValueObjects;

namespace Bwadl.API.Models.Responses;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    UserType Type,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
