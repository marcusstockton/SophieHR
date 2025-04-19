#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using SophieHR.Api.Extensions;
using SophieHR.Api.Interfaces;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;
using System.Security.Claims;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ILogger<CompaniesController> _logger;
        private readonly ICompanyService _companyService;
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompaniesController(ILogger<CompaniesController> logger, ICompanyService companyService, IDistributedCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _companyService = companyService;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/Companies
        [HttpGet, Authorize(Roles = "Admin"), Produces(typeof(IEnumerable<CompanyDetailNoLogo>))]
        public async Task<ActionResult<IEnumerable<CompanyDetailNoLogo>>> GetCompanies()
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Getting companies for admin user");

            var cacheKey = "companies";
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            var companies = await _cache.GetOrSetAsync(cacheKey, async () => {
                _logger.LogInformation($"Cache miss. Fetching data for key: {cacheKey} from database.");
                return await _companyService.GetAllCompaniesNoLogoAsync();
            }, cacheOptions);

            return Ok(companies);
        }

        [HttpGet("GetCompanyNamesForSelect"), Authorize(Policy = "CompanyManagement"), Produces(typeof(IEnumerable<KeyValuePair<Guid, string>>))]
        public async Task<ActionResult<IEnumerable<KeyValuePair<Guid, string>>>> GetCompanyNames()
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Getting company names");

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cacheKey = $"companyNames-{userId}";
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            var companyNames = await _cache.GetOrSetAsync(cacheKey, async () => {
                _logger.LogInformation($"Cache miss. Fetching data for key: {cacheKey} from database.");
                return await _companyService.GetCompanyNamesAsync(User.Identity.Name, (User.IsInRole("Manager") || User.IsInRole("CompanyAdmin")));
            }, cacheOptions);


            return Ok(companyNames);
        }

        // GET: api/Companies/5
        [HttpGet("{id}"), Produces(typeof(CompanyDetailDto)), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CompanyDetailDto>> GetCompany(Guid id)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Getting company with id {id}");

            var cacheKey = $"company:{id}";
            var company = await _cache.GetOrSetAsync(cacheKey,
            async () =>
            {
                _logger.LogInformation($"Cache miss. Fetching data for key: {cacheKey} from database.");
                return await _companyService.GetCompanyById(id);
            })!;

            if (company == null)
            {
                var error = $"Unable to find company with id {id}";
                _logger.LogWarning($"{nameof(CompaniesController)} {error}");
                return NotFound(error);
            }

            return Ok(company);
        }

        [HttpPost("{id}/upload-logo"), Authorize(Policy = "CompanyManagement")]
        [RequestFormLimits(MultipartBodyLengthLimit = 1000000)] // Limit to 1mb logo
        public async Task<IActionResult> UploadLogo(Guid id, IFormFile logo)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Uploading logo for company with id {id}");
            var response = await _companyService.UploadLogoForCompanyAsync(id, logo);
            if (response.IsSuccessStatusCode)
            {
                await _cache.RemoveAsync("companies");
                await _cache.RemoveAsync("companyNames");
                await _cache.RemoveAsync($"company:{id}");
                return Ok(response);
            }
            _logger.LogError($"{nameof(CompaniesController)} Error uploading logo for company with id {id}. Response: {response.Content.ReadAsStringAsync().Result}");
            return BadRequest(response);
        }

        // PUT: api/Companies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize(Policy = "CompanyManagement")]
        public async Task<HttpResponseMessage> PutCompany(Guid id, CompanyDetailNoLogo companyDetail)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Updating company with id {id}");
            await _cache.RemoveAsync("companies");
            await _cache.RemoveAsync("companyNames");
            await _cache.RemoveAsync($"company:{id}");
            
            return await _companyService.UpdateCompanyAsync(id, companyDetail);
        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CompanyDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostCompany(CompanyCreateDto companyDto)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Creating a new company with name {companyDto.Name}");
            try
            {
                var result = await _companyService.CreateNewCompanyAsync(companyDto);
                await _cache.RemoveAsync("companies");
                await _cache.RemoveAsync("companyNames");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CompaniesController)} Error creating a new company with name {companyDto.Name}");
                return BadRequest(ex);
            }
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<HttpResponseMessage> DeleteCompany(Guid id)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Deleting company with id {id}");
            await _cache.RemoveAsync("companies");
            await _cache.RemoveAsync("companyNames");
            await _cache.RemoveAsync($"company:{id}");
            return await _companyService.DeleteCompanyAsync(id);
        }

        [HttpGet, Route("get-location-autosuggestion")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAutoSuggestion(string search)
        {
            return Ok(await _companyService.GetAutoSuggestion(search));
        }

        [HttpGet, Route("GetMapFromLatLong")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMapFromLatLong(decimal lat, decimal lon, int zoomLevel = 15, int width = 2048, int height = 200)
        {
            var cacheKey = $"company-map-{lat}-{lon}-{zoomLevel}";
            var image = await _cache.GetOrSetAsync(cacheKey,
            async () =>
            {
                return await _companyService.GetMapFromLatLong(lat, lon, zoomLevel, width, height);
            })!;

            if (image.Length > 0)
            {
                return Ok(image);
            }
            return BadRequest();
        }

        [HttpGet, Route("postcode-auto-complete")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostcodeAutoComplete(string postcode)
        {
            var cacheKey = $"postcode:{postcode}";
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            var postcodes = await _cache.GetOrSetAsync(cacheKey,
            async () =>
            {
                _logger.LogInformation($"Cache miss. Fetching data for key: {cacheKey} from database.");
                return await _companyService.PostcodeAutoComplete(postcode);
            }, cacheOptions);

            return Ok(postcodes);
        }

        [ProducesResponseType(200)]
        [HttpGet, Route("postcode-lookup")]
        [ProducesResponseType(typeof(PostcodeLookup), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostcodeLookup(string postcode)
        {
            var cacheKey = $"{postcode}";
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            var postcodeData = await _cache.GetOrSetAsync(cacheKey,
            async () =>
            {
                _logger.LogInformation($"Cache miss. Fetching data for key: {cacheKey} from database.");
                return await _companyService.PostCodeLookup(postcode);
            }, cacheOptions);

            
            return Ok(postcodeData);
        }

        [HttpGet, Route("GetCompanyEmployeeCountByMonth")]
        [ProducesResponseType(typeof(List<CompanyEmployeeCount>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCompanyEmployeeCountByMonth(Guid companyId)
        {
            var result = await _cache.GetOrSetAsync($"employee_count_{companyId}",
            async () =>
            {
                return await _companyService.GetCompanyEmployeeCountByMonth(companyId);
            })!;

            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }
    }
}