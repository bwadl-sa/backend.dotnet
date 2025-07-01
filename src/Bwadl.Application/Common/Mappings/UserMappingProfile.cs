using AutoMapper;
using Bwadl.Application.Common.DTOs;
using Bwadl.Domain.Entities;

namespace Bwadl.Application.Common.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}
