using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;

namespace SophieHR.Api.Profiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<EmployeeDetailDto, Employee>().ReverseMap();
            CreateMap<EmployeeListDto, Employee>().ReverseMap();
            CreateMap<EmployeeCreateDto, Employee>().ReverseMap();
        }
    }
}
