#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SophieHR.Api.Models.DTOs.Company;
using SophieHR.Api.Services;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
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
                _logger.LogWarning($"{nameof(CompaniesController)} Unable to find company with id {id}");
                return NotFound();
            }

            return Ok(company);
        }

        [HttpPost("{id}/upload-logo"), Authorize(Roles = "Admin")]
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
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCompany(Guid id, CompanyDetailNoLogo companyDetail)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Updating company with id {id}");
            if (id != companyDetail.Id)
            {
                _logger.LogWarning($"{nameof(CompaniesController)} Id's don't match");
                return BadRequest();
            }
            var result = await _companyService.UpdateCompanyAsync(id, companyDetail);
            if (result.IsSuccessStatusCode)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<ActionResult<CompanyDetailDto>> PostCompany(CompanyCreateDto companyDto)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Creating a new company with name {companyDto.Name}");
            var result = await _companyService.CreateNewCompanyAsync(companyDto);
            if (result!=null)
            {
                return CreatedAtAction(nameof(GetCompany), new { id=result.Id }, result);
            }
            return BadRequest(result);
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            _logger.LogInformation($"{nameof(CompaniesController)} Deleting company with id {id}");
            var result = await _companyService.DeleteCompanyAsync(id);
            if (result.IsSuccessStatusCode)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}