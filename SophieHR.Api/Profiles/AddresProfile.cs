using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Address;

namespace SophieHR.Api.Profiles
{
    public class AddresProfile : Profile
    {
        public AddresProfile()
        {
            //CreateMap<CompanyAddress, Address>().ReverseMap();
            //CreateMap<EmployeeAddress, Address>().ReverseMap();
            CreateMap<AddressCreateDto, CompanyAddress>().ReverseMap();
            CreateMap<AddressCreateDto, EmployeeAddress>().ReverseMap();
        }
    }
}
