#nullable disable

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(ApplicationDbContext context, IMapper mapper, ILogger<CompaniesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Companies
        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CompanyDetailNoLogo>>> GetCompanies()
        {
            _logger.LogInformation("Getting companies for admin user");
            return _mapper.Map<List<CompanyDetailNoLogo>>(await _context.Companies.ToListAsync());
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDetailDto>> GetCompany(Guid id)
        {
            _logger.LogInformation($"Getting company with id {id}");
            var company = await _context.Companies
                .Include(x => x.Address)
                .Include(x => x.Employees)
                .AsNoTracking()
                .Select(x => new CompanyDetailDto
                {
                    Address = x.Address,
                    CreatedDate = x.CreatedDate,
                    EmployeeCount = x.Employees.Count(),
                    Id = x.Id,
                    Logo = x.Logo != null ? Convert.ToBase64String(x.Logo) : null,
                    Name = x.Name,
                    UpdatedDate = x.UpdatedDate
                })
                .FirstAsync(x => x.Id == id);

            if (company == null)
            {
                _logger.LogWarning($"Unable to find company with id {id}");
                return NotFound();
            }

            return Ok(company);
        }

        [HttpPost("{id}/upload-logo"), Authorize(Roles = "Admin")]
        [RequestFormLimits(MultipartBodyLengthLimit = 1000000)] // Limit to 1mb logo
        public async Task<IActionResult> UploadLogo(Guid id, IFormFile logo)
        {
            _logger.LogInformation($"Uploading logo for company with id {id}");
            if (logo != null)
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    _logger.LogWarning($"Unable to find company with id {id}");
                    return NotFound($"Unable to find a company with the Id of {id}");
                }

                // Resize the image to be 256 * 256...
                using (var memoryStream = new MemoryStream())
                {
                    await logo.CopyToAsync(memoryStream);
                    byte[] bytes = memoryStream.ToArray();

                    company.Logo = bytes;
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        // PUT: api/Companies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCompany(Guid id, CompanyDetailNoLogo companyDetail)
        {
            _logger.LogInformation($"Updating company with id {id}");
            if (id != companyDetail.Id)
            {
                _logger.LogWarning($"Id's don't match");
                return BadRequest();
            }
            var originalCompany = await _context.Companies.FindAsync(id);
            if (originalCompany == null)
            {
                _logger.LogWarning($"Unable to find original company with id {id}");
                return NotFound($"Unable to find a company with the Id of {id}");
            }
            var company = _mapper.Map(companyDetail, originalCompany);
            _context.Companies.Attach(company);
            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<ActionResult<CompanyDetailDto>> PostCompany(CompanyCreateDto companyDto)
        {
            _logger.LogInformation($"Creating a new company with name {companyDto.Name}");
            var company = _mapper.Map<Company>(companyDto);
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompany", new { id = company.Id }, company);
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            _logger.LogInformation($"Deleting company with id {id}");
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                _logger.LogWarning($"Unable to find company with id {id}");
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompanyExists(Guid id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}