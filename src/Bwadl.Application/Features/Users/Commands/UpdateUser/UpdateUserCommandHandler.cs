using AutoMapper;
using Bwadl.Application.Common.DTOs;
using Bwadl.Domain.Exceptions;
using Bwadl.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bwadl.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper, ILogger<UpdateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting user update for ID: {UserId}", request.Id);

        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User update failed - user not found with ID: {UserId}", request.Id);
            throw new UserNotFoundException(request.Id);
        }

        _logger.LogInformation("Found user {UserId}, checking for email conflicts", request.Id);

        // Check if email is being changed and if it already exists
        if (user.Email != request.Email && 
            await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("User update failed - email already exists: {Email} for user {UserId}", request.Email, request.Id);
            throw new DuplicateEmailException(request.Email);
        }

        _logger.LogInformation("Updating user {UserId} with name: {Name}, email: {Email}, type: {Type}", 
            request.Id, request.Name, request.Email, request.Type);

        user.UpdateName(request.Name);
        user.UpdateEmail(request.Email);
        user.UpdateType(request.Type);

        var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User {UserId} updated successfully", updatedUser.Id);
        return _mapper.Map<UserDto>(updatedUser);
    }
}
