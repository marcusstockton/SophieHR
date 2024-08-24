#nullable disable

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Services;
using System.Globalization;
using System.Security.Claims;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Manager, CompanyAdmin, HRManager")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _context;
        public readonly IMapper _mapper;
        private readonly ILogger<EmployeesController> _logger;
        private readonly IJobTitleService _jobTitleService;

        public EmployeesController(IEmployeeService context, IMapper mapper, ILogger<EmployeesController> logger, IJobTitleService jobTitleService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _jobTitleService = jobTitleService;
        }

        // GET: api/Employees
        [Authorize(Policy = "CompanyManagement")]
        [HttpGet("get-by-company/{companyId}")]
        [Produces(typeof(IEnumerable<EmployeeListDto>))]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetEmployeesForCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetEmployeesForCompanyId)} getting employees for company {companyId}");
            var employeeList = await _context.GetEmployeesForCompanyId(companyId);

            return Ok(employeeList);
        }

        [Authorize(Policy = "CompanyManagement")]
        [HttpGet("list-of-managers-for-company/{companyId}")]
        [Produces(typeof(IEnumerable<EmployeeListDto>))]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetManagersForCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetManagersForCompanyId)} Getting managers for company id {companyId}");
            // If user is a manager, just return them....
            if (User.IsInRole("Manager"))
            {
                var username = User.FindFirstValue(ClaimTypes.Name);
                var user = await _context.GetEmployeeByUsername(username);
                var result = new List<EmployeeListDto>();
                result.Add(new EmployeeListDto { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName });
                return Ok(result);
            }

            var managers = await _context.GetManagersForCompanyId(companyId);

            return Ok(managers);
        }

        [HttpGet("list-of-employees-for-manager/{managerId}")]
        [Produces(typeof(IEnumerable<EmployeeListDto>))]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetEmployeesForManager(Guid managerId)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetEmployeesForManager)} Getting employees for manager id {managerId}");
            var employees = await _context.GetEmployeesForManager(managerId);

            return Ok(employees);
        }

        // GET: api/Employees/5
        [HttpGet("get-by-id/{id}"), Authorize(Roles = "Admin, Manager, User, CompanyAdmin")]
        [Produces(typeof(EmployeeDetailDto)), ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "CompanyManagement")]
        public async Task<ActionResult<EmployeeDetailDto>> GetEmployee(Guid id)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetEmployee)} Getting employees by id {id}");
            var employee = await _context.GetEmployeeById(id, User);

            if (employee == null)
            {
                _logger.LogInformation($"No employee found by id {id}");
                return NotFound();
            }

            return Ok(_mapper.Map<EmployeeDetailDto>(employee));
        }

        [HttpPost("{id}/upload-avatar"), Authorize(Roles = "Admin, Manager")]
        [Consumes("multipart/form-data")]
        [Produces(typeof(EmployeeAvatar)), ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
        [RequestFormLimits(MultipartBodyLengthLimit = 5000000)] // Limit to 5mb
        public async Task<ActionResult<EmployeeAvatar>> UploadAvatar(Guid id, [FromForm] IFormFile avatar)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(UploadAvatar)} Uploading avatar for employee id {id}");
            if (avatar != null)
            {
                var employeeAvatar = await _context.UploadAvatarToEmployee(id, avatar);
                return Ok(employeeAvatar);
            }
            return BadRequest("No File");
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Produces(typeof(EmployeeDetailDto))]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployeeDetailDto>> PutEmployee([FromRoute]Guid id, [FromBody]EmployeeDetailDto employeeDetail)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(PutEmployee)} Updating employee {employeeDetail}");
            if (id != employeeDetail.Id)
            {
                return BadRequest();
            }
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if ((!User.IsInRole("Admin") && userid == id.ToString())) // Admin user can...
            {
                return BadRequest("Cannot update your own record");
            }

            var employee = await _context.UpdateEmployee(employeeDetail);
            if (employee == null)
            {
                return StatusCode(500);
            }

            return Ok(employee);
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin, Manager, CompanyAdmin, HRManager")]
        [HttpPost("create-employee")]
        [ProducesResponseType(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest), Produces(typeof(EmployeeDetailDto))]
        public async Task<ActionResult<EmployeeDetailDto>> CreateEmployee(EmployeeCreateDto employeeDto, string role = "User")
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(CreateEmployee)} creating employee {employeeDto}");
            Employee manager = null;

            if (role.ToLower() == "user" && !User.IsInRole("Manager"))
            {
                if (string.IsNullOrEmpty(employeeDto.ManagerId) || !Guid.TryParse(employeeDto.ManagerId, out var managerGuid))
                {
                    ModelState.AddModelError(employeeDto.ManagerId, "You need to supply a manager id");
                    return BadRequest(ModelState);
                }
                else
                {
                    manager = await _context.GetEmployeeById(managerGuid, User);
                }
            }

            if (User.IsInRole("Manager"))
            {
                // Default manager to current user:
                //var username = User.FindFirstValue(ClaimTypes.Name);
                //manager = await _context.GetEmployeeByUsername(username);
                //employeeDto.ManagerId = manager.Id.ToString();
                role = "User"; // Managers can't create other managers...
            }
            if (User.IsInRole("CompanyAdmin") && role.ToLower() == "admin")
            {
                ModelState.AddModelError(role, "You do not have the permission to create this type of user");
                return BadRequest(ModelState);
            }
            if (User.IsInRole("HRManager") && new[] { "admin", "companyadmin", "hrmanager" }.Any(c => role.Contains(c.ToLower())))
            {
                ModelState.AddModelError(role, "You do not have the permission to create this type of user");
                return BadRequest(ModelState);
            }
            try
            {
                var employee = await _context.CreateEmployee(employeeDto, manager, role);

                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(DeleteEmployee)} deleting employee {id}");

            await _context.DeleteEmployee(id);

            return NoContent();
        }

        [HttpGet("GetTitles")]
        [Produces(typeof(List<string>))]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        public ActionResult<List<string>> GetTitles()
        {
            _logger.LogInformation($"{nameof(EmployeesController)} > {nameof(GetTitles)} getting titles");
            return Ok(_context.GetTitles());
        }

        [HttpGet("job-title-autocomplete")]
        [Produces(typeof(List<string>))]
        public async Task<ActionResult> JobTitleAutoComplete(string jobTitle)
        {
            _logger.LogInformation($"{nameof(JobTitleAutoComplete)} finding job titles with {jobTitle}");
            var jobTitles = await _jobTitleService.JobTitlesAsync();

            var jobTitlesFiltered = jobTitles.Where(x => x.Contains(jobTitle, StringComparison.CurrentCultureIgnoreCase)).Select(x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x)).ToList();
            return Ok(jobTitlesFiltered);
        }
    }
}