using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Interfaces;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;

namespace SophieHR.Api.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;
        private readonly ILogger<CompanyService> _logger;
        private string _apiKey;
        private string _ukLatLong;
        private string _countryCode;
        private readonly IHttpClientFactory _httpClientFactory;

        public CompanyService(ApplicationDbContext context, IMapper mapper, ILogger<CompanyService> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _apiKey = Environment.GetEnvironmentVariable("HERE_Maps_API_Key");
            _ukLatLong = "55.3781,3.4360"; // UK lat/lon
            _countryCode = "GBP";
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ICollection<CompanyDetailNoLogo>> GetAllCompaniesNoLogoAsync()
        {
            _logger.LogInformation($"{nameof(GetAllCompaniesNoLogoAsync)} called");
            var companyList = await _context.Companies
                .AsNoTracking()
                .Include(x => x.Address)
                .ProjectTo<CompanyDetailNoLogo>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return companyList;
        }

        public async Task<ICollection<KeyValuePair<Guid, string>>> GetCompanyNamesAsync(string username, bool isManager = false)
        {
            _logger.LogInformation($"{nameof(GetCompanyNamesAsync)} called");
            var companies = await _context.Companies.AsNoTracking().Select(x => new KeyValuePair<Guid, string>(x.Id, x.Name)).ToListAsync();
            if (isManager)
            {
                var companyId = await _context.Employees.AsNoTracking().Where(x => x.UserName == username).Select(x => x.CompanyId).SingleOrDefaultAsync();
                companies = companies.Where(x => x.Key == companyId).ToList();
            }
            return companies;
        }

        public async Task<CompanyDetailDto> GetCompanyById(Guid id)
        {
            _logger.LogInformation($"{nameof(GetCompanyById)} called");
            try
            {
                var company = await _context.Companies
                .Include(x => x.Address)
                .Include(x => x.Employees)
                .Include(x => x.CompanyConfig)
                .AsNoTracking()
                //.ProjectTo<CompanyDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Id == id);

                return _mapper.Map<CompanyDetailDto>(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public async Task<Company> FindCompanyByIdAsync(Guid id)
        {
            _logger.LogInformation($"{nameof(FindCompanyByIdAsync)} called");
            return await _context.Companies.FindAsync(id);
        }

        public async Task<HttpResponseMessage> UpdateCompanyAsync(Guid id, CompanyDetailNoLogo companyDetail)
        {
            _logger.LogInformation($"{nameof(UpdateCompanyAsync)} called");
            if (companyDetail.Id != id)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent($"Id's do not match!") };
            }
            var originalCompany = await _context.Companies
                .Include(x => x.CompanyConfig)
                .Include(x => x.Address)
                .Include(x => x.Employees)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id);

            if (originalCompany == null)
            {
                _logger.LogWarning($"{nameof(UpdateCompanyAsync)} Unable to find company with id {id}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent($"Unable to find company with id {id}") };
            }

            try
            {
                var updatedCompany = _mapper.Map<Company>(companyDetail);
                updatedCompany.Logo = originalCompany.Logo;

                _context.Attach(updatedCompany);
                _context.Entry(updatedCompany).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception was thrown when trying to update Company id {id}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> UploadLogoForCompanyAsync(Guid id, IFormFile logo)
        {
            _logger.LogInformation($"{nameof(UploadLogoForCompanyAsync)} called");
            var result = new HttpResponseMessage();
            if (logo != null)
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    _logger.LogWarning($"{nameof(UpdateCompanyAsync)} Unable to find company with id {id}");
                    //return NotFound($"Unable to find a company with the Id of {id}");
                    result.StatusCode = System.Net.HttpStatusCode.NotFound;
                    result.Content = new StringContent($"Unable to find company with id {id}");
                    return result;
                }

                using (var memoryStream = new MemoryStream())
                {
                    await logo.CopyToAsync(memoryStream);
                    byte[] bytes = memoryStream.ToArray();

                    company.Logo = bytes;
                    await _context.SaveChangesAsync();
                }
            }
            result.StatusCode = System.Net.HttpStatusCode.NoContent;
            result.Content = new StringContent("Sucessfully added logo");
            return result;
        }

        public async Task<CompanyDetailDto> CreateNewCompanyAsync(CompanyCreateDto companyDto)
        {
            _logger.LogInformation($"{nameof(CreateNewCompanyAsync)} called");
            var company = _mapper.Map<Company>(companyDto);
            if (_context.Companies.Any(x => x.Name.Equals(companyDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Company with this name already exists!");
            }
            _context.Companies.Add(company);
            try
            {
                await _context.SaveChangesAsync();
                return _mapper.Map<CompanyDetailDto>(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving new company");
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteCompanyAsync(Guid companyId)
        {
            _logger.LogInformation($"{nameof(DeleteCompanyAsync)} called");
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
            {
                _logger.LogWarning($"Unable to find company with id {companyId}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"Unable to find company with id {companyId}") };
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
        }

        public async Task<string> GetAutoSuggestion(string search)
        {
            _logger.LogInformation($"{nameof(GetAutoSuggestion)} Getting autosuggestions for {search}");
            var client = _httpClientFactory.CreateClient("autosuggestHereApiClient");

            var url = $"?at={_ukLatLong}&countryCode={_countryCode}&limit=50&lang=en&q={search}&apiKey={_apiKey}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return data;
            }

            return null;
        }

        public async Task<string> GetMapFromLatLong(decimal lat, decimal lon, int zoomLevel = 15, int mapType = 3, int width = 2048, short viewType = 1)
        {
            if (_apiKey == null)
            {
                _logger.LogError("API Key is null");
                return string.Empty;
            }
            _logger.LogInformation($"{nameof(GetMapFromLatLong)} Getting Map for lat lon {lat} {lon}");
            var height = 300;
            var client = _httpClientFactory.CreateClient("imageHereApiClient");
            var url = $"?apiKey={_apiKey}&c={lat},{lon}&vt={viewType}&z={zoomLevel}&h={height}&w={width}";
            var response = await client.GetByteArrayAsync(url);

            return Convert.ToBase64String(response);
        }

        public async Task<string[]> PostcodeAutoComplete(string postcode)
        {
            _logger.LogInformation($"{nameof(PostcodeAutoComplete)} querying postcode {postcode}");

            var client = _httpClientFactory.CreateClient("postcodesioClient");
            var url = $"{postcode}/autocomplete";
            var response = await client.GetFromJsonAsync<PostcodeAutoComplate>(url);
            if (response.status == 200)
            {
                return response.result;
            }

            return null;
        }

        public async Task<PostcodeLookup> PostCodeLookup(string postcode)
        {
            _logger.LogInformation($"{nameof(PostcodeAutoComplete)} querying postcode {postcode}");
            if (await VerifyPostcode(postcode))
            {
                var client = _httpClientFactory.CreateClient("postcodesioClient");
                var url = $"{postcode}";
                var response = await client.GetFromJsonAsync<PostcodeLookup>(url);
                if (response.status == 200)
                {
                    return response;
                }
                else
                {
                    throw new ArgumentException("Invalid Postcode");
                }
            }
            throw new ArgumentException("Invalid Postcode supplied");
        }

        private async Task<bool> VerifyPostcode(string postcode)
        {
            var client = _httpClientFactory.CreateClient("postcodesioClient");
            var url = $"{postcode}/validate";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadFromJsonAsync<VerifyPostcodeResult>();
                return jsonData.Result;
            }
            return false;
        }

        private class PostcodeAutoComplate
        {
            public int status { get; set; }
            public string[] result { get; set; }
        }
    }
}