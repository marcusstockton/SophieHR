#nullable disable

using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Department;

namespace SophieHR.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;

        public DepartmentsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Departments
        [HttpGet, Authorize(Roles ="Admin")]
        public async Task<ActionResult<IEnumerable<DepartmentDetailDto>>> GetDepartments()
        {
            return _mapper.Map<List<DepartmentDetailDto>>(await _context.Departments.ToListAsync());
        }

        [HttpGet("get-departments-by-companyid/{companyId}"), Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<IEnumerable<DepartmentDetailDto>>> GetDepartmentsByCompanyId(Guid companyId)
        {
            return _mapper.Map<List<DepartmentDetailDto>>(await _context.Departments.Where(x => x.CompanyId == companyId).ToListAsync());
        }

        // GET: api/Departments/5
        [HttpGet("get-department-by-id/{id}")]
        public async Task<ActionResult<DepartmentDetailDto>> GetDepartment(Guid id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return _mapper.Map<DepartmentDetailDto>(department);
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> PutDepartment(Guid id, DepartmentDetailDto departmentDetail)
        {
            if (id != departmentDetail.Id)
            {
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
        [HttpPost, Authorize(Roles = "Admin, Manager", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<DepartmentDetailDto>> PostDepartment(DepartmentCreateDto departmentCreateDto)
        {
            var department = _mapper.Map<Department>(departmentCreateDto);
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDepartment", new { id = department.Id }, _mapper.Map<DepartmentDetailDto>(department));
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin, Manager", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
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