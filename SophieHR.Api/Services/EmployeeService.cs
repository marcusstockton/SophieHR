﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using System.Security.Claims;

namespace SophieHR.Api.Services
{
    public interface IEmployeeService
    {
        Task<ICollection<EmployeeListDto>> GetEmployeesForCompanyId(Guid companyId);

        Task<ICollection<EmployeeListDto>> GetManagersForCompanyId(Guid companyId);

        Task<ICollection<EmployeeListDto>> GetEmployeesForManager(Guid managerId);

        Task<EmployeeDetailDto> GetEmployeeById(Guid employeeId, ClaimsPrincipal user);

        Task UploadAvatarToEmployee(Guid id, IFormFile avatar);

        Task UpdateEmployee(EmployeeDetailDto employeeDto);

        Task CreateEmployee(EmployeeCreateDto employeeDto);

        Task DeleteEmployee(Guid employeeId);

        ICollection<string> GetTitles();
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<CompanyService> _logger;

        public EmployeeService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<CompanyService> logger)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task CreateEmployee(EmployeeCreateDto employeeDto)
        {
            _logger.LogInformation($"{nameof(CreateEmployee)} called.");
            var employee = _mapper.Map<Employee>(employeeDto);

            employee.UserName = employeeDto.WorkEmailAddress;
            var newEmployee = await _context.Employees.AddAsync(employee);
            await _userManager.CreateAsync(employee, "P@55w0rd1");
            await _userManager.AddToRoleAsync(employee, "User");
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployee(Guid employeeId)
        {
            _logger.LogInformation($"{nameof(DeleteEmployee)} called.");
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                throw new ArgumentException("Unable to find employee");
            }

            await _userManager.DeleteAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<EmployeeDetailDto> GetEmployeeById(Guid employeeId, ClaimsPrincipal user)
        {
            _logger.LogInformation($"{nameof(GetEmployeeById)} called.");
            var employee = await _context.Employees
               .Include(x => x.Avatar)
               .Include(x => x.Address)
               .Include(x => x.Department)
               .Include(x => x.Company)
               .Include(x => x.Manager)
               .SingleOrDefaultAsync(x => user.IsInRole("User") ? x.UserName == user.Identity.Name : x.Id == employeeId);

            return _mapper.Map<EmployeeDetailDto>(employee);
        }

        public async Task<ICollection<EmployeeListDto>> GetEmployeesForCompanyId(Guid companyId)
        {
            _logger.LogInformation($"{nameof(GetEmployeesForCompanyId)} called.");
            var employeeList = await _context.Employees
                .Include(x => x.Department)
                .Where(x => x.CompanyId == companyId)
                .ToListAsync();
            return _mapper.Map<ICollection<EmployeeListDto>>(employeeList);
        }

        public async Task<ICollection<EmployeeListDto>> GetEmployeesForManager(Guid managerId)
        {
            _logger.LogInformation($"{nameof(GetEmployeesForManager)} called.");
            var employees = await _context.Employees
                .Include(x => x.Department)
                .Where(x => x.Manager.Id == managerId)
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

        public async Task UpdateEmployee(EmployeeDetailDto employeeDto)
        {
            _logger.LogInformation($"{nameof(UpdateEmployee)} called.");
            var originalEmployee = await _context.Employees.FindAsync(employeeDto.Id);
            if (originalEmployee == null)
            {
                throw new ArgumentException($"Unable to find a employee with the Id of {employeeDto.Id}");
            }
            var employee = _mapper.Map(employeeDto, originalEmployee);
            _context.Employees.Attach(employee);
            _context.Entry(employee).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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