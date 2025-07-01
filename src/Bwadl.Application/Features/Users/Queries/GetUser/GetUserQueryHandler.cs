using AutoMapper;
using Bwadl.Application.Common.DTOs;
using Bwadl.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bwadl.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserQueryHandler> _logger;

    public GetUserQueryHandler(IUserRepository userRepository, IMapper mapper, ILogger<GetUserQueryHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving user with ID: {UserId}", request.Id);

        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (user == null)
        {
            _logger.LogInformation("User not found with ID: {UserId}", request.Id);
            return null;
        }

        _logger.LogInformation("User found with ID: {UserId}, Name: {Name}", user.Id, user.Name);
        return _mapper.Map<UserDto>(user);
    }
}
