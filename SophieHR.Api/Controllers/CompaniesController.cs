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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;

        public CompaniesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Companies
        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CompanyDetailNoLogo>>> GetCompanies()
        {
            return _mapper.Map<List<CompanyDetailNoLogo>>(await _context.Companies.ToListAsync());
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDetailDto>> GetCompany(Guid id)
        {
            var company = await _context.Companies
                .Include(x => x.Address)
                .AsNoTracking()
                .FirstAsync(x => x.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            return _mapper.Map<CompanyDetailDto>(company);
        }

        [HttpPost("{id}/upload-logo"), Authorize(Roles = "Admin")]
        [RequestFormLimits(MultipartBodyLengthLimit = 1000000)] // Limit to 1mb logo
        public async Task<IActionResult> UploadLogo(Guid id, IFormFile logo)
        {
            if (logo != null)
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
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
            if (id != companyDetail.Id)
            {
                return BadRequest();
            }
            var originalCompany = await _context.Companies.FindAsync(id);
            if (originalCompany == null)
            {
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
            var company = _mapper.Map<Company>(companyDto);
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompany", new { id = company.Id }, company);
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
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