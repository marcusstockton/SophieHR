using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Models;
using System.Security.Claims;

namespace SophieHR.Api.Interfaces
{
    public interface IEmployeeService
    {
        Task<ICollection<EmployeeListDto>> GetEmployeesForCompanyId(Guid companyId);

        Task<ICollection<EmployeeListDto>> GetManagersForCompanyId(Guid companyId);

        Task<ICollection<EmployeeListDto>> GetEmployeesForManager(Guid managerId);

        Task<Employee> GetEmployeeById(Guid employeeId, ClaimsPrincipal user);

        Task<Employee> GetEmployeeByUsername(string username);

        Task<EmployeeAvatar> UploadAvatarToEmployee(Guid id, IFormFile avatar);

        Task<EmployeeDetailDto> UpdateEmployee(EmployeeDetailDto employeeDto);

        Task<Employee> CreateEmployee(EmployeeCreateDto employeeDto, Employee? manager = null, string role = "User");

        Task DeleteEmployee(Guid employeeId);

        ICollection<string> GetTitles();
    }
}
