#nullable disable

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Department;
using ApiExplorerSettingsAttribute = Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(ApplicationDbContext context, IMapper mapper, ILogger<DepartmentsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Departments
        [HttpGet, Authorize(Roles = "Admin"), Produces(typeof(IEnumerable<DepartmentDetailDto>))]
        public async Task<ActionResult<IEnumerable<DepartmentDetailDto>>> GetDepartments()
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(GetDepartments)} getting Departments");
            return _mapper.Map<List<DepartmentDetailDto>>(await _context.Departments.ToListAsync());
        }

        [HttpGet("get-departments-by-companyid/{companyId}"), Authorize(Roles = "Admin, Manager"), Produces(typeof(IEnumerable<DepartmentDetailDto>))]
        public async Task<ActionResult<IEnumerable<DepartmentDetailDto>>> GetDepartmentsByCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(GetDepartmentsByCompanyId)} getting Departments for company {companyId}");

            return _mapper.Map<List<DepartmentDetailDto>>(await _context.Departments.Where(x => x.CompanyId == companyId).ToListAsync());
        }

        // GET: api/Departments/5
        [HttpGet("get-department-by-id/{id}"), Produces(typeof(ActionResult<DepartmentDetailDto>)), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartmentDetailDto>> GetDepartment(Guid id)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(GetDepartment)} getting Department by id {id}");
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                _logger.LogError($"{nameof(DepartmentsController)} > {nameof(GetDepartment)} failed...Unable to find department by Id {id}");
                return NotFound();
            }

            return _mapper.Map<DepartmentDetailDto>(department);
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize(Roles = "Admin, Manager"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutDepartment(Guid id, DepartmentDetailDto departmentDetail)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(PutDepartment)} updating Department {departmentDetail.Name} against companyid {departmentDetail.CompanyId}");
            if (id != departmentDetail.Id)
            {
                _logger.LogError($"{nameof(DepartmentsController)} > {nameof(PutDepartment)} failed...Id's don't match");
                return BadRequest();
            }
            var department = _mapper.Map<Department>(departmentDetail);
            _context.Entry(department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
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

        // POST: api/Departments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(StatusCodes.Status201Created), ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DepartmentDetailDto>> PostDepartment(DepartmentCreateDto departmentCreateDto)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(PostDepartment)} creating Department {departmentCreateDto.Name} against companyid {departmentCreateDto.CompanyId}");
            if (!_context.Companies.Any(x => x.Id == departmentCreateDto.CompanyId))
            {
                return BadRequest("Please select an existing company");
            }
            if(_context.Departments.Where(x=>x.CompanyId == departmentCreateDto.CompanyId && x.Name == departmentCreateDto.Name).Any())
            {
                return BadRequest("A Department with this name already exists!");
            }
            var department = _mapper.Map<Department>(departmentCreateDto);
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            var dept = _mapper.Map<DepartmentDetailDto>(department);
            return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, dept);
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin, Manager"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(DeleteDepartment)} deleting Department id {id}");

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                _logger.LogError($"{nameof(DepartmentsController)} > {nameof(DeleteDepartment)} failed...Unable to find department by Id {id}");
                return NotFound();
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DepartmentExists(Guid id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}