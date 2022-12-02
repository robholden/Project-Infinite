
using AutoMapper;

using Content.Domain;
using Content.Domain.Dtos;

namespace Content.Api;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<UserSettingDto, UserSetting>().ReverseMap();
        CreateMap<CountryDto, Country>().ReverseMap();
        CreateMap<BoundryDto, Boundry>().ReverseMap();
        CreateMap<TagDto, Tag>().ReverseMap();
        CreateMap<TagAdminDto, Tag>().ReverseMap();
        CreateMap<LocationDto, Location>().ReverseMap();
        CreateMap<CollectionDto, Collection>().ReverseMap();
        CreateMap<PictureModerationDto, PictureModeration>().ReverseMap();

        MapPictureMap<PictureUserDto>();
        MapPictureMap<PictureDto>();
    }

    private void MapPictureMap<T>() where T : PictureDto
    {
        CreateMap<T, Picture>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => new Tag() { Value = t }).ToList()))
            .ReverseMap()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => t.Value)))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UpdatedDate.HasValue ? src.Name : ""));
    }
}