using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Models.DTOs.Employee.EmployeeAvatar;

namespace SophieHR.Api.Profiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeDetailDto>().ReverseMap();
            CreateMap<EmployeeListDto, Employee>().ReverseMap();
            CreateMap<EmployeeCreateDto, Employee>().ReverseMap();
            CreateMap<EmployeeAvatarDetail, EmployeeAvatar>().ReverseMap();
            
            CreateMap<string?, byte[]?>().ConvertUsing<Base64Converter>();
            CreateMap<byte[]?, string?>().ConvertUsing<Base64Converter>();
        }

        private class Base64Converter : ITypeConverter<string?, byte[]?>, ITypeConverter<byte[]?, string?>
        {
            public byte[]? Convert(string? source, byte[]? destination, ResolutionContext context)
                => string.IsNullOrEmpty(source) ? null : System.Convert.FromBase64String(source);

            public string? Convert(byte[]? source, string? destination, ResolutionContext context)
                => source != null && source.Length > 0 ? System.Convert.ToBase64String(source) : null;
        }
    }
}
