using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Department;

namespace SophieHR.Api.Profiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            CreateMap<Department, DepartmentCreateDto>().ReverseMap();
            CreateMap<Department, DepartmentDetailDto>().ReverseMap();
            CreateMap<Department, DepartmentIdNameDto>().ReverseMap();
        }
    }
}