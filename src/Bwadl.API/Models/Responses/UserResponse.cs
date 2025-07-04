using Bwadl.Domain.ValueObjects;
using Bwadl.Application.Common.DTOs;

namespace Bwadl.API.Models.Responses;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    UserType Type,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// V2 API Response Models
public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class UserDetailResponse
{
    public UserDto User { get; set; } = null!;
    public UserMetadata Metadata { get; set; } = null!;
}

public class UserMetadata
{
    public double ProfileCompleteness { get; set; }
    public int LastLoginDays { get; set; }
    public TimeSpan AccountAge { get; set; }
}
