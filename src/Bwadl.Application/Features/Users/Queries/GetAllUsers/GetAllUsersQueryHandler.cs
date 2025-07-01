using AutoMapper;
using Bwadl.Application.Common.DTOs;
using Bwadl.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bwadl.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper, ILogger<GetAllUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all users");

        var users = await _userRepository.GetAllAsync(cancellationToken);
        var userList = users.ToList();
        
        _logger.LogInformation("Retrieved {UserCount} users", userList.Count);
        return _mapper.Map<IEnumerable<UserDto>>(userList);
    }
}
