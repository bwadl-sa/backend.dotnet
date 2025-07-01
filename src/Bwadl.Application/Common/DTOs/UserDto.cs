using Bwadl.Domain.ValueObjects;

namespace Bwadl.Application.Common.DTOs;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    UserType Type,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
