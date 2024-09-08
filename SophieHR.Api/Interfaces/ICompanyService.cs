using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;

namespace SophieHR.Api.Interfaces
{
    public interface ICompanyService
    {
        Task<ICollection<CompanyDetailNoLogo>> GetAllCompaniesNoLogoAsync();

        Task<ICollection<KeyValuePair<Guid, string>>> GetCompanyNamesAsync(string username, bool isManager = false);

        Task<CompanyDetailDto> GetCompanyById(Guid id);

        Task<Company> FindCompanyByIdAsync(Guid id);

        Task<HttpResponseMessage> UpdateCompanyAsync(Guid id, CompanyDetailNoLogo companyDetail);

        Task<HttpResponseMessage> UploadLogoForCompanyAsync(Guid id, IFormFile logo);

        Task<CompanyDetailDto> CreateNewCompanyAsync(CompanyCreateDto companyDto);

        Task<HttpResponseMessage> DeleteCompanyAsync(Guid companyId);

        Task<string> GetAutoSuggestion(string search);

        Task<string> GetMapFromLatLong(decimal lat, decimal lon, int zoomLevel = 15, int mapType = 3, int width = 2048, short viewType = 1);

        Task<string[]> PostcodeAutoComplete(string postcode);

        Task<PostcodeLookup> PostCodeLookup(string postcode);
    }
}
