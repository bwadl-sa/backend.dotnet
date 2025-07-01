using Bwadl.Application.Common.DTOs;
using MediatR;

namespace Bwadl.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;
