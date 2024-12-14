using AutoMapper;
using Baigiamasis.DTOs.HumanInformation;
using Baigiamasis.Models;

namespace Baigiamasis.Mappings
{
    public class HumanInformationMappingProfile : Profile
    {
        public HumanInformationMappingProfile()
        {
            CreateMap<CreateHumanInformationDto, HumanInformation>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    Id = Guid.NewGuid(),
                    City = src.City,
                    Street = src.Street,
                    HouseNumber = src.HouseNumber,
                    ApartmentNumber = src.ApartmentNumber
                }));

            CreateMap<HumanInformation, HumanInformationDto>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
                .ForMember(dest => dest.HouseNumber, opt => opt.MapFrom(src => src.Address.HouseNumber))
                .ForMember(dest => dest.ApartmentNumber, opt => opt.MapFrom(src => src.Address.ApartmentNumber))
                .ForMember(dest => dest.ProfilePictureBase64, opt => opt.MapFrom(src => 
                    src.ProfilePicture != null ? Convert.ToBase64String(src.ProfilePicture) : null));

            CreateMap<HumanInformationDto, HumanInformation>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    City = src.City,
                    Street = src.Street,
                    HouseNumber = src.HouseNumber,
                    ApartmentNumber = src.ApartmentNumber
                }));
        }
    }
} 