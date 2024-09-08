using SophieHR.Api.Models;

namespace SophieHR.Api.Interfaces
{
    public interface IDepartmentService
    {
        Task<ICollection<Department>> GetDepartmentsForCompanyId(Guid companyId);

        Task<Department> GetDepartmentById(Guid departmentId);

        Task<Department> UpdateDepartment(Guid id, Department departmentDetail);

        Task<Department> CreateDepartment(Department departmentCreate);

        Task DeleteDepartment(Guid id);
    }
}
