using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.User;

namespace SophieHR.Api.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, ApplicationUser>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.Firstname))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.Lastname))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.EmailAddress))
                .ReverseMap();
        }
    }
}
