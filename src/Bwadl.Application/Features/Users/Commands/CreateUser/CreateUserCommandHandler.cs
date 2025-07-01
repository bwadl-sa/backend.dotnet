using AutoMapper;
using Bwadl.Application.Common.DTOs;
using Bwadl.Domain.Entities;
using Bwadl.Domain.Exceptions;
using Bwadl.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bwadl.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(IUserRepository userRepository, IMapper mapper, ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting user creation for email: {Email}", request.Email);

        // Check if user with email already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("User creation failed - email already exists: {Email}", request.Email);
            throw new DuplicateEmailException(request.Email);
        }

        _logger.LogInformation("Creating new user with name: {Name}, email: {Email}, type: {Type}", 
            request.Name, request.Email, request.Type);

        var user = new User(request.Name, request.Email, request.Type);
        
        _logger.LogInformation("Saving user to repository with ID: {UserId}", user.Id);
        var createdUser = await _userRepository.AddAsync(user, cancellationToken);

        _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);
        return _mapper.Map<UserDto>(createdUser);
    }
}
