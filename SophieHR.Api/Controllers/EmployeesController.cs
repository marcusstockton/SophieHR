#nullable disable

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Manager")]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(ApplicationDbContext context, IMapper mapper, ILogger<EmployeesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Employees
        [HttpGet("get-by-company/{companyId}")]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetEmployeesForCompanyId(Guid companyId)
        {
            var employeeList = await _context.Employees.Where(x => x.CompanyId == companyId).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<EmployeeListDto>>(employeeList));
        }

        [HttpGet("list-of-managers-for-company/{companyId}")]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetManagersForCompanyId(Guid companyId)
        {
            _logger.LogInformation($"Getting managers for company id {companyId}");
            var managers = _context.Employees.Where(x => x.CompanyId == companyId);

            var managerRoleId = _context.Roles.Single(x => x.Name == "Manager").Id;
            var userroles = await _context.UserRoles.Where(x => x.RoleId == managerRoleId && managers.Select(x => x.Id).Contains(x.UserId)).Select(x => x.UserId).ToListAsync();
            var managerList = await managers.Where(x => userroles.Contains(x.Id)).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<EmployeeListDto>>(managerList));
        }

        [HttpGet("list-of-employees-for-manager/{managerId}")]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetEmployeesForManager(Guid managerId)
        {
            _logger.LogInformation($"Getting employees for manager id {managerId}");
            var employees = await _context.Employees.Where(x => x.Manager.Id == managerId).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<EmployeeListDto>>(employees));
        }

        // GET: api/Employees/5
        [HttpGet("get-by-id/{id}"), Authorize(Roles = "Admin, Manager, User")]
        public async Task<ActionResult<EmployeeDetailDto>> GetEmployee(Guid id)
        {
            _logger.LogInformation($"Getting employees by id {id}");
            var employee = await _context.Employees
                .Include(x => x.Avatar)
                .Include(x => x.Address)
                .Include(x => x.Department)
                .Include(x => x.Company)
                .SingleOrDefaultAsync(x => User.IsInRole("User") ? x.UserName == User.Identity.Name : x.Id == id); // If user is user role, return their record only

            if (employee == null)
            {
                _logger.LogInformation($"No employee found by id {id}");
                return NotFound();
            }
            return Ok(_mapper.Map<EmployeeDetailDto>(employee));
        }

        [HttpPost("{id}/upload-avatar"), Authorize(Roles = "Admin, Manager")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5000000)] // Limit to 5mb logo
        public async Task<IActionResult> UploadAvatar(Guid id, IFormFile avatar)
        {
            _logger.LogInformation($"Uploading avatar for employee id {id}");
            if (avatar != null)
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning($"Unable to find a employee with the Id of {id}");
                    return NotFound($"Unable to find a employee with the Id of {id}");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await avatar.CopyToAsync(memoryStream);
                    byte[] bytes = memoryStream.ToArray();

                    employee.Avatar = new EmployeeAvatar { Avatar = bytes };
                    _context.Employees.Update(employee);
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(Guid id, EmployeeDetailDto employeeDetail)
        {
            if (id != employeeDetail.Id)
            {
                return BadRequest();
            }
            var originalEmployee = await _context.Employees.FindAsync(id);
            if (originalEmployee == null)
            {
                return NotFound($"Unable to find a employee with the Id of {id}");
            }
            var employee = _mapper.Map(employeeDetail, originalEmployee);
            _context.Employees.Attach(employee);
            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeeDetailDto>> PostEmployee(EmployeeCreateDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(Guid id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}