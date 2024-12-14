using AutoMapper;
using Baigiamasis.DTOs.User;
using Baigiamasis.Models;

namespace Baigiamasis.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.HasHumanInformation,
                    opt => opt.MapFrom(src => src.HumanInformation != null));

            CreateMap<UserRegistrationDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => new List<string> { "User" }));
        }
    }
} 