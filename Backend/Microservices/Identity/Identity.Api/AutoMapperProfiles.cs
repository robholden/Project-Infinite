using AutoMapper;

using Identity.Domain;
using Identity.Domain.Dtos;

namespace Identity.Api;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AuthTokenDto, AuthToken>().ReverseMap();
        CreateMap<RecoveryCodeDto, RecoveryCode>().ReverseMap();
        CreateMap<UserDto, User>().ReverseMap();
        CreateMap<SimpleUserDto, User>().ReverseMap();
        CreateMap<AdminUserDto, User>().ReverseMap();
    }
}