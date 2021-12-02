using AutoMapper;

using Comms.Api.Dtos;
using Comms.Domain;

namespace Comms.Api;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<UserSettingDto, UserSetting>().ReverseMap();
        CreateMap<NotificationDto, Notification>().ReverseMap()
            .ForMember(dest => dest.Users, opt => opt.MapFrom(src => CountUsers(src)))
            .ForMember(dest => dest.IsGlobal, opt => opt.MapFrom(src => src.UserId == new System.Guid()));
    }

    private static int CountUsers(Notification notification)
    {
        return notification.Entries?.Count ?? 0;
    }
}