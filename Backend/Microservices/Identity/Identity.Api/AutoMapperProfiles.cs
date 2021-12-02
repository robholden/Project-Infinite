using AutoMapper;

using Identity.Api.Dtos;
using Identity.Domain;

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