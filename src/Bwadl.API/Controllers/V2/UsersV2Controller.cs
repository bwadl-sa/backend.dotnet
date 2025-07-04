using Asp.Versioning;
using Bwadl.Application.Common.DTOs;
using Bwadl.Application.Features.Users.Queries.GetAllUsers;
using Bwadl.Application.Features.Users.Queries.GetUser;
using Bwadl.API.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bwadl.API.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/users")]
[Route("api/users")]
public class UsersV2Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersV2Controller> _logger;

    public UsersV2Controller(IMediator mediator, ILogger<UsersV2Controller> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with pagination (V2 feature)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserDto>>> GetAllUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("GET /api/v2/users - Retrieving users with pagination. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetAllUsersQuery();
        var users = await _mediator.Send(query);

        // V2 adds pagination logic
        var totalCount = users.Count();
        var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var response = new PagedResponse<UserDto>
        {
            Data = pagedUsers,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        _logger.LogInformation("GET /api/v2/users - Returning {UserCount} users (Page {Page} of {TotalPages})", 
            pagedUsers.Count, page, response.TotalPages);

        return Ok(response);
    }

    /// <summary>
    /// Get user by ID with additional metadata (V2 feature)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailResponse>> GetUser(Guid id)
    {
        _logger.LogInformation("GET /api/v2/users/{UserId} - Retrieving user with metadata", id);

        var query = new GetUserQuery(id);
        var user = await _mediator.Send(query);

        if (user == null)
        {
            _logger.LogInformation("GET /api/v2/users/{UserId} - User not found", id);
            return NotFound();
        }

        // V2 adds metadata
        var response = new UserDetailResponse
        {
            User = user,
            Metadata = new UserMetadata
            {
                ProfileCompleteness = CalculateProfileCompleteness(user),
                LastLoginDays = CalculateLastLoginDays(user),
                AccountAge = DateTime.UtcNow - user.CreatedAt
            }
        };

        _logger.LogInformation("GET /api/v2/users/{UserId} - User found with metadata", user.Id);
        return Ok(response);
    }

    private static double CalculateProfileCompleteness(UserDto user)
    {
        // Simple completeness calculation
        var fields = new[] { user.Name, user.Email };
        var completedFields = fields.Count(f => !string.IsNullOrWhiteSpace(f));
        return (double)completedFields / fields.Length * 100;
    }

    private static int CalculateLastLoginDays(UserDto user)
    {
        // Mock calculation - in real app, you'd have LastLogin property
        return (DateTime.UtcNow - user.CreatedAt).Days;
    }
}
