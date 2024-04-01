using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;

namespace SophieHR.Api.Services
{
    public interface IDepartmentService
    {
        Task<ICollection<Department>> GetDepartmentsForCompanyId(Guid companyId);

        Task<Department> GetDepartmentById(Guid departmentId);

        Task<Department> UpdateDepartment(Guid id, Department departmentDetail);

        Task<Department> CreateDepartment(Department departmentCreate);

        Task DeleteDepartment(Guid id);
    }

    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private ILogger<DepartmentService> _logger;

        public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Department> CreateDepartment(Department department)
        {
            if (!_context.Companies.Any(x => x.Id == department.CompanyId))
            {
                throw new BadHttpRequestException("Please select an existing company");
            }
            if (_context.Departments.Where(x => x.CompanyId == department.CompanyId && x.Name == department.Name).Any())
            {
                throw new BadHttpRequestException("A Department with this name already exists!");
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task DeleteDepartment(Guid id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Department> GetDepartmentById(Guid departmentId)
        {
            return await _context.Departments.FindAsync(departmentId);
        }

        public async Task<ICollection<Department>> GetDepartmentsForCompanyId(Guid companyId)
        {
            return await _context.Departments.Where(x => x.CompanyId == companyId).ToListAsync();
        }

        public async Task<Department> UpdateDepartment(Guid id, Department departmentDetail)
        {
            _context.Entry(departmentDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return departmentDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department");
                throw;
            }
        }
    }
}