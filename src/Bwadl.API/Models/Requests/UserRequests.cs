using Bwadl.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Bwadl.API.Models.Requests;

public record CreateUserRequest(
    [Required][StringLength(100, MinimumLength = 1)]
    string Name,
    
    [Required][EmailAddress][StringLength(255)]
    string Email,
    
    [Required]
    UserType Type
);

public record UpdateUserRequest(
    [Required][StringLength(100, MinimumLength = 1)]
    string Name,
    
    [Required][EmailAddress][StringLength(255)]
    string Email,
    
    [Required]
    UserType Type
);
