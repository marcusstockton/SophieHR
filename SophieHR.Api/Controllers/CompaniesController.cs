#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SophieHR.Api.Models.DTOs.Company;
using SophieHR.Api.Services;

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

        public CompaniesController(ILogger<CompaniesController> logger, ICompanyService companyService)
        {
            _logger = logger;
            _companyService = companyService;
        }

        // GET: api/Companies
        [HttpGet, Authorize(Roles = "Admin"), Produces(typeof(IEnumerable<CompanyDetailNoLogo>))]
        public async Task<ActionResult<IEnumerable<CompanyDetailNoLogo>>> GetCompanies()
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Getting companies for admin user");
            return Ok(await _companyService.GetAllCompaniesNoLogoAsync());
        }

        [HttpGet("GetCompanyNamesForSelect"), Authorize(Roles = "Admin, Manager"), Produces(typeof(IEnumerable<KeyValuePair<Guid, string>>))]
        public async Task<ActionResult<IEnumerable<KeyValuePair<Guid, string>>>> GetCompanyNames()
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Getting company names");
            return Ok(await _companyService.GetCompanyNamesAsync(User.Identity.Name, User.IsInRole("Manager")));
        }

        // GET: api/Companies/5
        [HttpGet("{id}"), Produces(typeof(CompanyDetailDto)), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CompanyDetailDto>> GetCompany(Guid id)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Getting company with id {id}");
            var company = await _companyService.GetCompanyByIdNoTrackingAsync(id);

            if (company == null)
            {
                var error = $"Unable to find company with id {id}";
                _logger.LogWarning($"{nameof(CompaniesController)} {error}");
                return NotFound();
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
                return Ok(response);
            }
            return BadRequest(response);
        }

        // PUT: api/Companies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize(Policy = "CompanyManagement")]
        public async Task<HttpResponseMessage> PutCompany(Guid id, CompanyDetailNoLogo companyDetail)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Updating company with id {id}");
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
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<HttpResponseMessage> DeleteCompany(Guid id)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Deleting company with id {id}");
            return await _companyService.DeleteCompanyAsync(id);
        }

        //[AllowAnonymous]
        [HttpGet, Route("get-location-autosuggestion"), ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAutoSuggestion(string search)
        {
            return Ok(await _companyService.GetAutoSuggestion(search));
        }

        //[AllowAnonymous]
        [HttpGet, Route("GetMapFromLatLong"), ResponseCache(Duration = 86400)]// One day
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMapFromLatLong(decimal lat, decimal lon, int zoomLevel = 15, int mapType = 3, int width = 2048, short viewType = 1)
        {
            var result = await _companyService.GetMapFromLatLong(lat, lon, zoomLevel, mapType, width, viewType);
            if(result.Length > 0)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        //[AllowAnonymous]
        [HttpGet, Route("postcode-auto-complete"), ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostcodeAutoComplete(string postcode)
        {
            var result = await _companyService.PostcodeAutoComplete(postcode);
            return Ok(result);
        }

        //[AllowAnonymous]
        [ProducesResponseType(200)]
        [HttpGet, Route("postcode-lookup"), ResponseCache(Duration = 300)] // 5 mins
        [ProducesResponseType(typeof(PostcodeLookup), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostcodeLookup(string postcode)
        {
            var result = await _companyService.PostCodeLookup(postcode);
            return Ok(result);
        }
    }
}