using Bwadl.Application.Common.DTOs;
using Bwadl.Domain.ValueObjects;
using MediatR;

namespace Bwadl.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Name,
    string Email,
    UserType Type
) : IRequest<UserDto>;
