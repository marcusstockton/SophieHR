using AutoMapper;
using SophieHR.Api.Extensions;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Models.DTOs.Employee.EmployeeAvatar;

#nullable enable

namespace SophieHR.Api.Profiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeDetailDto>().ReverseMap();
            //.ForMember(x => x.Avatar.Avatar, opt => opt.MapFrom(src => Convert.ToBase64String(src.Avatar.Avatar)))
            //.ReverseMap()
            //.ForMember(x => x.Avatar.Avatar, opt => opt.MapFrom(src => Convert.FromBase64String(src.Avatar.Avatar)));

            CreateMap<Employee, EmployeeListDto>().ReverseMap();
            CreateMap<Employee, EmployeeCreateDto>()
                .ForMember(x => x.ManagerId, opt => opt.MapFrom(src => src.Manager.Id))
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(x => x.WorkEmailAddress, opt => opt.MapFrom(src => src.Email))
                .IgnoreAllVirtual()
                .ReverseMap();

            CreateMap<EmployeeCreateDto, Employee>()
                .ForMember(x => x.Manager, opt => opt.Ignore());

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