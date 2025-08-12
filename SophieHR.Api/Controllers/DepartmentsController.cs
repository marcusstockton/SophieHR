#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SophieHR.Api.Interfaces;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Department;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _context;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(IDepartmentService context, ILogger<DepartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("get-departments-by-companyid/{companyId}"), Authorize(Policy = "CompanyManagement"), Produces(typeof(IEnumerable<DepartmentDetailDto>))]
        public async Task<ActionResult<IEnumerable<DepartmentDetailDto>>> GetDepartmentsByCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(GetDepartmentsByCompanyId)} getting Departments for company {companyId}");

            var departments = await _context.GetDepartmentsForCompanyId(companyId);
            var results = departments.Select(d => new DepartmentDetailDto
            {
                Id = d.Id,
                Name = d.Name,
                CompanyId = companyId
            }).ToList();

            return Ok(results);
        }

        // GET: api/Departments/5
        [HttpGet("get-department-by-id/{id}"), Produces(typeof(ActionResult<DepartmentDetailDto>)), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartmentDetailDto>> GetDepartment(Guid id)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(GetDepartment)} getting Department by id {id}");
            var department = await _context.GetDepartmentById(id);

            if (department == null)
            {
                var errorMessage = $"Department with Id {id} not found.";
                //_logger.LogError($"{nameof(DepartmentsController)} > {nameof(GetDepartment)} failed...{errorMessage}");
                _logger.LogError("{nameof} > {nameofdept} failed...{errorMessage}", nameof(DepartmentsController), nameof(GetDepartment), errorMessage);
                //return NotFound();
                return Problem(detail: errorMessage, statusCode: StatusCodes.Status404NotFound);

            }

            var results = new DepartmentDetailDto
            {
                Id = department.Id,
                Name = department.Name,
                CompanyId = department.Company.Id
            };

            return Ok(results);
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize(Policy = "CompanyManagement"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutDepartment(Guid id, DepartmentDetailDto departmentDetail)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(PutDepartment)} updating Department {departmentDetail.Name} against companyid {departmentDetail.CompanyId}");
            if (id != departmentDetail.Id)
            {
                _logger.LogError($"{nameof(DepartmentsController)} > {nameof(PutDepartment)} failed...Id's don't match");
                //return BadRequest();
                return Problem(detail: "ID's do not match. Check your inputs", statusCode: StatusCodes.Status400BadRequest);

            }
            var department = new Department
            {
                Id = departmentDetail.Id,
                Name = departmentDetail.Name,
                CompanyId = departmentDetail.CompanyId,
            };

            await _context.UpdateDepartment(id, department);

            return NoContent();
        }

        // POST: api/Departments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize(Policy = "CompanyManagement")]
        [ProducesResponseType(StatusCodes.Status201Created), ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DepartmentDetailDto>> PostDepartment(DepartmentCreateDto departmentCreateDto)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(PostDepartment)} creating Department {departmentCreateDto.Name} against companyid {departmentCreateDto.CompanyId}");

            var department = new Department
            {
                Name = departmentCreateDto.Name,
                CompanyId = departmentCreateDto.CompanyId
            };
            await _context.CreateDepartment(department);

            var dept = new DepartmentDetailDto
            {
                Id = department.Id,
                Name = department.Name,
                CompanyId = department.CompanyId
            };
            return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, dept);
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}"), Authorize(Policy = "CompanyManagement"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            _logger.LogInformation($"{nameof(DepartmentsController)} > {nameof(DeleteDepartment)} deleting Department id {id}");

            await _context.DeleteDepartment(id);

            return NoContent();
        }
    }
}