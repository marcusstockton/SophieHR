using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;

#nullable enable

namespace SophieHR.Api.Profiles
{
    public class CompanyProfile : Profile
    {
        public CompanyProfile()
        {
            //CreateMap<string, byte[]>().ConvertUsing(x=> Convert.FromBase64String(x));
            //CreateMap<byte[], string>().ConvertUsing(x=>Convert.ToBase64String(x));
            CreateMap<Company, CompanyCreateDto>().ReverseMap();

            CreateMap<Company, CompanyDetailDto>()
                .ForMember(x=>x.Logo, src=>src.MapFrom(opt=> opt.Logo.Any() ? Convert.ToBase64String(opt.Logo) : ""))
                .ForMember(x=>x.EmployeeCount, opt =>opt.MapFrom(src=>src.Employees.Count));
            
            CreateMap<CompanyDetailDto, Company>()
                .ForMember(x=>x.Logo, src =>src.MapFrom(opt => !string.IsNullOrEmpty(opt.Logo) ? Convert.FromBase64String(opt.Logo): new byte[0]));

            CreateMap<Company, CompanyDetailNoLogo>().ReverseMap();
            CreateMap<Company, CompanyIdNameDto>().ReverseMap();
        }

        //private class Base64Converter : ITypeConverter<string?, byte[]?>, ITypeConverter<byte[]?, string?>
        //{
        //    public byte[]? Convert(string? source, byte[]? destination, ResolutionContext context)
        //        => string.IsNullOrEmpty(source) ? null : System.Convert.FromBase64String(source);

        //    public string? Convert(byte[]? source, string? destination, ResolutionContext context)
        //        => source != null && source.Length > 0 ? System.Convert.ToBase64String(source) : null;
        //}
    }
}