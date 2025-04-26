using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = "CompanyManagement")]
    public class CompanyConfigController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CompanyConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CompanyConfig/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyConfig>> GetCompanyConfig(Guid id)
        {
            var companyConfig = await _context.CompanyConfigs.Where(c=>c.CompanyId == id).FirstOrDefaultAsync();

            if (companyConfig == null)
            {
                return NotFound();
            }

            return companyConfig;
        }

        // PUT: api/CompanyConfig/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompanyConfig(Guid id, CompanyConfig companyConfig)
        {
            if (id != companyConfig.Id)
            {
                return BadRequest();
            }

            _context.Entry(companyConfig).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyConfigExists(id))
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

        // POST: api/CompanyConfig
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CompanyConfig>> PostCompanyConfig(CompanyConfig companyConfig)
        {
            _context.CompanyConfigs.Add(companyConfig);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompanyConfig", new { id = companyConfig.Id }, companyConfig);
        }

        // DELETE: api/CompanyConfig/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyConfig(Guid id)
        {
            var companyConfig = await _context.CompanyConfigs.FindAsync(id);
            if (companyConfig == null)
            {
                return NotFound();
            }

            _context.CompanyConfigs.Remove(companyConfig);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompanyConfigExists(Guid id)
        {
            return _context.CompanyConfigs.Any(e => e.Id == id);
        }
    }
}
