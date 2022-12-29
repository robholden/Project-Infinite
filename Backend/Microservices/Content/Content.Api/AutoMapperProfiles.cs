
using AutoMapper;

using Content.Domain;
using Content.Domain.Dtos;

namespace Content.Api;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<PostDto, Post>().ReverseMap();
    }
}