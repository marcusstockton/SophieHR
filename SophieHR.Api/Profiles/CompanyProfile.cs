using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;

namespace SophieHR.Api.Profiles
{
    public class CompanyProfile : Profile
    {
        public CompanyProfile()
        {
            CreateMap<CompanyCreateDto, Company>().ReverseMap();
            CreateMap<CompanyDetailDto, Company>().ReverseMap();
            CreateMap<CompanyDetailNoLogo, Company>().ReverseMap();

            CreateMap<string, byte[]>().ConvertUsing<Base64Converter>();
            CreateMap<byte[], string>().ConvertUsing<Base64Converter>();
        }

        private class Base64Converter : ITypeConverter<string, byte[]>, ITypeConverter<byte[], string>
        {
            public byte[] Convert(string source, byte[] destination, ResolutionContext context)
                => System.Convert.FromBase64String(source);

            public string Convert(byte[] source, string destination, ResolutionContext context)
                => System.Convert.ToBase64String(source);
        }
    }
}