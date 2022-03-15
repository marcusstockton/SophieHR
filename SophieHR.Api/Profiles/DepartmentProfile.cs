using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Department;

namespace SophieHR.Api.Profiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            CreateMap<DepartmentCreateDto, Department>().ReverseMap();
            CreateMap<DepartmentDetailDto, Department>().ReverseMap();
            CreateMap<DepartmentIdNameDto, Department>().ReverseMap();
        }
    }
}