#nullable disable

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeesController(ApplicationDbContext context, IMapper mapper, ILogger<EmployeesController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: api/Employees
        [HttpGet("get-by-company/{companyId}"), Produces(typeof(IEnumerable<EmployeeListDto>))]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetEmployeesForCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetEmployeesForCompanyId)} getting employees for company {companyId}");
            var employeeList = await _context.Employees
                .Include(x => x.Department)
                .Where(x => x.CompanyId == companyId)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<EmployeeListDto>>(employeeList));
        }

        [HttpGet("list-of-managers-for-company/{companyId}"), Produces(typeof(IEnumerable<EmployeeListDto>))]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetManagersForCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetManagersForCompanyId)} Getting managers for company id {companyId}");
            var managers = _context.Employees.Where(x => x.CompanyId == companyId);

            var managerRoleId = _context.Roles.Single(x => x.Name == "Manager").Id;
            var userroles = await _context.UserRoles
                .Where(x => x.RoleId == managerRoleId && managers.Select(x => x.Id)
                .Contains(x.UserId))
                .Select(x => x.UserId)
                .ToListAsync();

            var managerList = await managers
                .Where(x => userroles.Contains(x.Id))
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<EmployeeListDto>>(managerList));
        }

        [HttpGet("list-of-employees-for-manager/{managerId}"), Produces(typeof(IEnumerable<EmployeeListDto>))]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetEmployeesForManager(Guid managerId)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetEmployeesForManager)} Getting employees for manager id {managerId}");
            var employees = await _context.Employees
                .Where(x => x.Manager.Id == managerId)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<EmployeeListDto>>(employees));
        }

        // GET: api/Employees/5
        [HttpGet("get-by-id/{id}"), Authorize(Roles = "Admin, Manager, User"), Produces(typeof(EmployeeListDto))]
        public async Task<ActionResult<EmployeeDetailDto>> GetEmployee(Guid id)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetEmployee)} Getting employees by id {id}");
            var employee = await _context.Employees
                .Include(x => x.Avatar)
                .Include(x => x.Address)
                .Include(x => x.Department)
                .Include(x => x.Company)
                .Include(x => x.Manager)
                .SingleOrDefaultAsync(x => User.IsInRole("User") ? x.UserName == User.Identity.Name : x.Id == id); // If user is user role, return their record only

            if (employee == null)
            {
                _logger.LogInformation($"No employee found by id {id}");
                return NotFound();
            }
            return Ok(_mapper.Map<EmployeeDetailDto>(employee));
        }

        [HttpPost("{id}/upload-avatar"), Authorize(Roles = "Admin, Manager"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        [RequestFormLimits(MultipartBodyLengthLimit = 5000000)] // Limit to 5mb logo
        public async Task<IActionResult> UploadAvatar(Guid id, IFormFile avatar)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(UploadAvatar)} Uploading avatar for employee id {id}");
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
        [HttpPut("{id}"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutEmployee(Guid id, EmployeeDetailDto employeeDetail)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(PutEmployee)} Updating employee {employeeDetail}");
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
        [HttpPost, ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<EmployeeDetailDto>> PostEmployee(EmployeeCreateDto employeeDto)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(PostEmployee)} creating employee {employeeDto}");

            var employee = _mapper.Map<Employee>(employeeDto);
            //_context.Employees.Add(employee);

            await _userManager.CreateAsync(employee, "P@55w0rd1");
            await _userManager.AddToRoleAsync(employee, "User");
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(DeleteEmployee)} deleting employee {id}");

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(employee);
            //_context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("GetTitles"), Produces(typeof(List<string>))]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        public ActionResult<List<string>> GetTitles()
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetTitles)} getting titles");
            return Ok(Enum.GetNames(typeof(Title)).ToList());
        }

        private bool EmployeeExists(Guid id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }


    }
}