using Bwadl.Domain.Exceptions;
using Bwadl.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bwadl.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(IUserRepository userRepository, ILogger<DeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting user deletion for ID: {UserId}", request.Id);

        if (!await _userRepository.ExistsAsync(request.Id, cancellationToken))
        {
            _logger.LogWarning("User deletion failed - user not found with ID: {UserId}", request.Id);
            throw new UserNotFoundException(request.Id);
        }

        _logger.LogInformation("Deleting user with ID: {UserId}", request.Id);
        await _userRepository.DeleteAsync(request.Id, cancellationToken);
        
        _logger.LogInformation("User {UserId} deleted successfully", request.Id);
    }
}
