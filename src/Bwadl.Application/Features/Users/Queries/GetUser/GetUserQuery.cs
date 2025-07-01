using Bwadl.Application.Common.DTOs;
using MediatR;

namespace Bwadl.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(Guid Id) : IRequest<UserDto?>;
