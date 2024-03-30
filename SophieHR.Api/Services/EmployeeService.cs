using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using System.Data;
using System.Security.Claims;

namespace SophieHR.Api.Services
{
    public interface IEmployeeService
    {
        Task<ICollection<EmployeeListDto>> GetEmployeesForCompanyId(Guid companyId);

        Task<ICollection<EmployeeListDto>> GetManagersForCompanyId(Guid companyId);

        Task<ICollection<EmployeeListDto>> GetEmployeesForManager(Guid managerId);

        Task<Employee> GetEmployeeById(Guid employeeId, ClaimsPrincipal user);

        Task<Employee> GetEmployeeByUsername(string username);

        Task UploadAvatarToEmployee(Guid id, IFormFile avatar);

        Task<EmployeeDetailDto> UpdateEmployee(EmployeeDetailDto employeeDto);

        Task<Employee> CreateEmployee(EmployeeCreateDto employeeDto, Employee manager = null, string role = "User");

        Task DeleteEmployee(Guid employeeId);

        ICollection<string> GetTitles();
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, IMapper mapper, ILogger<EmployeeService> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Employee> CreateEmployee(EmployeeCreateDto employeeDto, Employee manager = null, string role = "User")
        {
            _logger.LogInformation($"{nameof(CreateEmployee)} called.");
            var employee = _mapper.Map<Employee>(employeeDto);
            if (!_context.Employees.Select(x=>x.WorkEmailAddress).Any(x => x == employee.WorkEmailAddress))
            {
                if (string.IsNullOrEmpty(employeeDto.ManagerId) && role == "User")
                {
                    throw new ArgumentNullException("ManagerId", "A valid managerId is required");
                }
                employee.Manager = _mapper.Map<Employee>(manager);
                employee.Email = employeeDto.WorkEmailAddress;
                var created = await _userManager.CreateAsync(employee, "P@55w0rd1");
                if (!created.Succeeded)
                {
                    foreach (var item in created.Errors)
                    {
                        _logger.LogError($"{nameof(CreateEmployee)} failed to create the user. {item.Code}:- {item.Description}");
                    }
                    
                    throw new ArgumentException( created.Errors.Select(x => x.Description).ToArray().ToString());
                }
                var userrole = await _roleManager.FindByNameAsync(role);
                if (userrole != null)
                {
                    IdentityResult roleResult = await _userManager.AddToRoleAsync(employee, userrole.Name);
                }
                try
                {
                    await _context.SaveChangesAsync();
                    return employee;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving the employee record");
                    throw;
                }
                
            }
            else
            {
                throw new ArgumentException("Employee already exists with this email adress");
            }
        }

        public async Task DeleteEmployee(Guid employeeId)
        {
            _logger.LogInformation($"{nameof(DeleteEmployee)} called.");
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                throw new ArgumentException("Unable to find employee");
            }

            //await _userManager.DeleteAsync(employee);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<Employee> GetEmployeeById(Guid employeeId, ClaimsPrincipal user)
        {
            _logger.LogInformation($"{nameof(GetEmployeeById)} called.");

            return await _context.Employees
               .Include(x => x.Avatar)
               .Include(x => x.Address)
               .Include(x => x.Department)
               .Include(x => x.Company)
               .Include(x => x.Manager)
               .AsNoTracking()
               .SingleOrDefaultAsync(x =>
                    user.IsInRole("User") ? x.UserName == user.Identity.Name
                    : x.Id == employeeId);
        }

        public async Task<Employee> GetEmployeeByUsername(string username)
        {
            _logger.LogInformation($"{nameof(GetEmployeeByUsername)} called.");
            return await _context.Employees
               .Include(x => x.Avatar)
               .Include(x => x.Address)
               .Include(x => x.Department)
               .Include(x => x.Company)
               .Include(x => x.Manager)
               .Include(x => x.Notes)
               .AsNoTracking()
               .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<ICollection<EmployeeListDto>> GetEmployeesForCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(GetEmployeesForCompanyId)} called.");
            var employeeList = await _context.Employees
                .Include(x => x.Department)
                .Where(x => x.CompanyId == companyId)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<ICollection<EmployeeListDto>>(employeeList);
        }

        public async Task<ICollection<EmployeeListDto>> GetEmployeesForManager(Guid managerId)
        {
            _logger.LogInformation($"{nameof(GetEmployeesForManager)} called.");
            var employees = await _context.Employees
                .Include(x => x.Department)
                .Include(x => x.Address)
                .Where(x => x.Manager.Id == managerId)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<ICollection<EmployeeListDto>>(employees);
        }

        public async Task<ICollection<EmployeeListDto>> GetManagersForCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(GetManagersForCompanyId)} called.");
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

            return _mapper.Map<ICollection<EmployeeListDto>>(managerList);
        }

        public ICollection<string> GetTitles()
        {
            _logger.LogInformation($"{nameof(GetTitles)} called.");
            return Enum.GetNames(typeof(Title)).ToList();
        }

        public async Task<EmployeeDetailDto> UpdateEmployee(EmployeeDetailDto employeeDto)
        {
            _logger.LogInformation($"{nameof(UpdateEmployee)} called.");

            var originalEmployee = await _context.Employees.FindAsync(employeeDto.Id);
            if (originalEmployee == null)
            {
                throw new ArgumentException($"Unable to find a employee with the Id of {employeeDto.Id}");
            }
            _mapper.Map(employeeDto, originalEmployee);
            _context.Employees.Update(originalEmployee);
            try
            {
                await _context.SaveChangesAsync();
                return _mapper.Map<EmployeeDetailDto>(originalEmployee);
            }
            catch (DBConcurrencyException ex)
            {
                _logger.LogError(ex, $"Database error occured when updating employee id {employeeDto.Id}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee id {employeeDto.Id}");
                return null;
            }
        }

        public async Task UploadAvatarToEmployee(Guid id, IFormFile avatar)
        {
            _logger.LogInformation($"{nameof(UploadAvatarToEmployee)} called.");
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                var message = $"Unable to find a employee with the Id of {id}";
                _logger.LogWarning(message);
                throw new ArgumentException(message);
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
    }
}