using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Interfaces;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Address;
using SophieHR.Api.Models.DTOs.Employee;
using System.Data;
using System.Security.Claims;

namespace SophieHR.Api.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, ILogger<EmployeeService> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<Employee> CreateEmployee(EmployeeCreateDto employeeDto, Employee? manager = null, string role = "User")
        {
            _logger.LogInformation($"{nameof(CreateEmployee)} called.");
            var employee = MapToEmployee(employeeDto);
            if (!_context.Employees.Select(x => x.WorkEmailAddress).Any(x => x == employee.WorkEmailAddress))
            {
                if (string.IsNullOrEmpty(employeeDto.ManagerId) && role == "User")
                {
                    throw new ArgumentNullException("ManagerId", "A valid managerId is required");
                }
                employee.Email = employeeDto.WorkEmailAddress;
                employee.UserName = employeeDto.Username;
                if (!string.IsNullOrEmpty(employeeDto.ManagerId))
                {
                    employee.ManagerId = Guid.Parse(employeeDto.ManagerId);
                }
                
                try
                {
                    var address = MapToEmployeeAddress(employeeDto.Address);
                    var addressSaved = await _context.EmployeeAddresses.AddAsync(address);
                    employee.AddressId = address.Id;
                    var created = await _userManager.CreateAsync(employee, "P@55w0rd1");
                    if (!created.Succeeded)
                    {
                        foreach (var item in created.Errors)
                        {
                            _logger.LogError($"{nameof(CreateEmployee)} failed to create the user. {item.Code}:- {item.Description}");
                        }

                        throw new ArgumentException(created.Errors.Select(x => x.Description).ToArray().ToString());
                    }
                    var userrole = await _roleManager.FindByNameAsync(role);
                    if (userrole != null)
                    {
                        IdentityResult roleResult = await _userManager.AddToRoleAsync(employee, userrole.Name);
                    }

                    await _context.SaveChangesAsync();
                    return employee;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to save the user");
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
               .FirstOrDefaultAsync(x =>
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
            return employeeList.Select(MapToEmployeeListDto).ToList();
        }

        public async Task<ICollection<EmployeeListDto>> GetEmployeesForManager(Guid managerId)
        {
            _logger.LogInformation($"{nameof(GetEmployeesForManager)} called.");

            var managerEmp = await _context.Employees.FindAsync(managerId);
            var roles = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == managerId);
            var role = await _context.Roles.FindAsync(roles.RoleId);

            var employeeList = new List<Employee>();

            if (role.Name.Equals("CompanyAdmin"))
            {
                employeeList = await _context.Employees.Include(x => x.Department)
                .Include(x => x.Address).Where(x => x.CompanyId == managerEmp.CompanyId).AsNoTracking().ToListAsync();
            }
            else
            {
                employeeList = await _context.Employees
                .Include(x => x.Department)
                .Include(x => x.Address)
                .Where(x => x.Manager.Id == managerId)
                .AsNoTracking()
                .ToListAsync();
            }

            return employeeList.Select(MapToEmployeeListDto).ToList();
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

            return managerList.Select(MapToEmployeeListDto).ToList();
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

            MapToExistingEmployee(employeeDto, originalEmployee);
            _context.Employees.Update(originalEmployee);

            try
            {
                await _context.SaveChangesAsync();
                return MapToEmployeeDetailDto(originalEmployee);
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

        public async Task<EmployeeAvatar> UploadAvatarToEmployee(Guid id, IFormFile avatar)
        {
            _logger.LogInformation($"{nameof(UploadAvatarToEmployee)} called.");
            var employee = await _context.Employees
                .Include(x => x.Avatar)
                .SingleAsync(x => x.Id == id);
            if (employee == null)
            {
                var message = $"Unable to find a employee with the Id of {id}";
                _logger.LogWarning(message);
                throw new ArgumentException(message);
            }

            if (employee.Avatar?.Avatar != null)
            {
                // existing avatar, so delete it!
                _context.EmployeeAvatars.Remove(employee.Avatar);
            }

            using (var memoryStream = new MemoryStream())
            {
                await avatar.CopyToAsync(memoryStream);
                byte[] bytes = memoryStream.ToArray();

                employee.Avatar = new EmployeeAvatar { Avatar = bytes };
                _context.Employees.Update(employee);
                try
                {
                    await _context.SaveChangesAsync();
                    return employee.Avatar;
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, $"{nameof(UploadAvatarToEmployee)} DbUpdateException exception thrown.");
                    return employee.Avatar;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(UploadAvatarToEmployee)} exception thrown.");
                    return employee.Avatar;
                }
            }
        }

        // Helper Methods for Manual Mapping
        private Employee MapToEmployee(EmployeeCreateDto dto)
        {
            return new Employee
            {
                UserName = dto.Username,
                Title = Enum.Parse<Title>(dto.Title),
                Gender = Enum.Parse<Gender>(dto.Gender),
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                WorkEmailAddress = dto.WorkEmailAddress,
                PersonalEmailAddress = dto.PersonalEmailAddress,
                WorkPhoneNumber = dto.WorkPhoneNumber,
                WorkMobileNumber = dto.WorkMobileNumber,
                PersonalMobileNumber = dto.PersonalMobileNumber,
                HolidayAllowance = dto.HolidayAllowance,
                DateOfBirth = dto.DateOfBirth,
                StartOfEmployment = dto.StartOfEmployment,
                PassportNumber = dto.PassportNumber,
                NationalInsuranceNumber = dto.NationalInsuranceNumber,
                DepartmentId = dto.DepartmentId,
                CompanyId = dto.CompanyId,
                JobTitle = dto.JobTitle,
                ManagerId = !string.IsNullOrEmpty(dto.ManagerId) ? Guid.Parse(dto.ManagerId) : null
            };
        }

        private EmployeeAddress MapToEmployeeAddress(AddressCreateDto dto)
        {
            return new EmployeeAddress
            {
                County = dto.County,
                Line1 = dto.Line1,
                Line2 = dto.Line2,
                Line3 = dto.Line3,
                Line4 = dto.Line4,
                Lat = dto.Lat,
                Lon = dto.Lon,
                Postcode = dto.Postcode,
            };
        }

        private EmployeeListDto MapToEmployeeListDto(Employee employee)
        {
            return new EmployeeListDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                LastName = employee.LastName,
                //Address = employee.Address,
                CompanyId = employee.CompanyId,
                DateOfBirth = employee.DateOfBirth,
                //DepartmentId = employee.DepartmentId,
                JobTitle = employee.JobTitle,
                //PersonalEmailAddress = employee.PersonalEmailAddress,
                //PersonalMobileNumber = employee.PersonalMobileNumber,
                WorkEmailAddress = employee.WorkEmailAddress,
                WorkMobileNumber = employee.WorkMobileNumber,
                HolidayAllowance = employee.HolidayAllowance,
                StartOfEmployment = employee.StartOfEmployment
            };
        }

        private EmployeeDetailDto MapToEmployeeDetailDto(Employee employee)
        {
            return new EmployeeDetailDto
            {
                Id = employee.Id,
                Title = employee.Title.ToString(),
                Gender = employee.Gender.ToString(),
                UserName = employee.UserName,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                LastName = employee.LastName,
                WorkEmailAddress = employee.WorkEmailAddress,
                PersonalEmailAddress = employee.PersonalEmailAddress,
                WorkPhoneNumber = employee.WorkPhoneNumber,
                WorkMobileNumber = employee.WorkMobileNumber,
                PersonalMobileNumber = employee.PersonalMobileNumber,
                HolidayAllowance = employee.HolidayAllowance,
                JobTitle = employee.JobTitle,
                DateOfBirth = employee.DateOfBirth,
                StartOfEmployment = employee.StartOfEmployment,
                EndOfEmployment = employee.EndOfEmployment,
                PassportNumber = employee.PassportNumber,
                NationalInsuranceNumber = employee.NationalInsuranceNumber,
                AddressId = employee.AddressId,
                ManagerId = employee.ManagerId,
                CompanyId = employee.CompanyId,
                DepartmentId = employee.DepartmentId,
                EmployeeAvatarId = employee.EmployeeAvatarId
            };
        }

        private void MapToExistingEmployee(EmployeeDetailDto dto, Employee employee)
        {
            employee.Title = Enum.Parse<Title>(dto.Title);
            employee.Gender = Enum.Parse<Gender>(dto.Gender);
            employee.UserName = dto.UserName;
            employee.FirstName = dto.FirstName;
            employee.MiddleName = dto.MiddleName;
            employee.LastName = dto.LastName;
            employee.WorkEmailAddress = dto.WorkEmailAddress;
            employee.PersonalEmailAddress = dto.PersonalEmailAddress;
            employee.WorkPhoneNumber = dto.WorkPhoneNumber;
            employee.WorkMobileNumber = dto.WorkMobileNumber;
            employee.PersonalMobileNumber = dto.PersonalMobileNumber;
            employee.HolidayAllowance = dto.HolidayAllowance;
            employee.JobTitle = dto.JobTitle;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.StartOfEmployment = dto.StartOfEmployment;
            employee.EndOfEmployment = dto.EndOfEmployment;
            employee.PassportNumber = dto.PassportNumber;
            employee.NationalInsuranceNumber = dto.NationalInsuranceNumber;
            employee.AddressId = dto.AddressId ?? employee.AddressId;
            employee.ManagerId = dto.ManagerId ?? employee.ManagerId;
            employee.CompanyId = dto.CompanyId ?? employee.CompanyId;
            employee.DepartmentId = dto.DepartmentId ?? employee.DepartmentId;
        }
    }
}