using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Profiles;

namespace SophieHR.Api.Services.Tests
{
    [TestClass()]
    public class DepartmentServiceTests
    {
        private ApplicationDbContext _context = default!;
        private DepartmentService _service = default!;

        private Guid CompanyID_1;
        private Guid CompanyID_2;

        private Guid DepartmentID_1;

        [TestInitialize()]
        public async Task SetupDataAsync()
        {
            var _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nameof(DepartmentServiceTests))
                .Options;

            _context = new ApplicationDbContext(_options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DepartmentProfile());
                cfg.AddProfile(new CompanyProfile());
            });
            var mapper = config.CreateMapper();

            var mockLogger = new Mock<ILogger<DepartmentService>>();

            CompanyID_1 = Guid.NewGuid();
            CompanyID_2 = Guid.NewGuid();
            var companyList = new List<Company>
            {
                new Company{ Id = CompanyID_1, Name = "Test Company One", Address = new CompanyAddress{ Line1 = "Line 1", Line2 = "Line 2", Postcode = "EX11EX" } },
                new Company{ Id = CompanyID_2, Name = "Test Company Two", Address = new CompanyAddress{ Line1 = "Line 1", Line2 = "Line 2", Postcode = "EX22EX" } },
            };
            await _context.Companies.AddRangeAsync(companyList);
            await _context.SaveChangesAsync();

            DepartmentID_1 = Guid.NewGuid();
            var departmentList = new List<Department>
            {
                new Department{Id = DepartmentID_1, Name = "Sales", CompanyId = CompanyID_1},
                new Department{Id = Guid.NewGuid(), Name = "Marketing", CompanyId = CompanyID_1},
                new Department{Id = Guid.NewGuid(), Name = "IT", CompanyId = CompanyID_1},
                new Department{Id = Guid.NewGuid(), Name = "Accounting", CompanyId = CompanyID_1},
                new Department{Id = Guid.NewGuid(), Name = "Sales", CompanyId = CompanyID_2},
                new Department{Id = Guid.NewGuid(), Name = "Marketing", CompanyId = CompanyID_2},
                new Department{Id = Guid.NewGuid(), Name = "IT", CompanyId = CompanyID_2},
                new Department{Id = Guid.NewGuid(), Name = "Accounting", CompanyId = CompanyID_2},
            };

            await _context.Departments.AddRangeAsync(departmentList);
            await _context.SaveChangesAsync();

            _service = new DepartmentService(_context, mockLogger.Object);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            _context.Dispose();
        }

        [TestMethod()]
        public async Task CreateDepartmentTestAsync()
        {
            var newDept = new Department
            {
                Name = "New Department",
                CompanyId = CompanyID_1
            };

            var result = await _service.CreateDepartment(newDept);
            Assert.AreEqual("New Department", result.Name);

            var departmentsCountForCompanyID_1 = _context.Departments.Where(x => x.CompanyId == CompanyID_1).Count();
            Assert.AreEqual(5, departmentsCountForCompanyID_1);
        }

        [TestMethod()]
        public async Task DeleteDepartmentTestAsync()
        {
            var salesDeptComp1 = await _context.Departments.SingleAsync(x => x.CompanyId == CompanyID_1 && x.Name == "Sales");

            var departmentsCountForCompanyID_1 = _context.Departments.Where(x => x.CompanyId == CompanyID_1).Count();
            Assert.AreEqual(4, departmentsCountForCompanyID_1);

            await _service.DeleteDepartment(salesDeptComp1.Id);

            var departmentsCountForCompanyID_1_AfterDelete = _context.Departments.Where(x => x.CompanyId == CompanyID_1).Count();
            Assert.AreEqual(3, departmentsCountForCompanyID_1_AfterDelete);
        }

        [TestMethod()]
        public async Task GetDepartmentByIdTestAsync()
        {
            var dept = await _service.GetDepartmentById(DepartmentID_1);
            Assert.AreEqual("Sales", dept.Name);
            Assert.AreEqual(CompanyID_1, dept.CompanyId);
        }

        [TestMethod()]
        public async Task GetDepartmentsForCompanyIdTestAsync()
        {
            var departmentList = await _service.GetDepartmentsForCompanyId(CompanyID_2);
            Assert.IsNotNull(departmentList);
            Assert.AreEqual(4, departmentList.Count());
        }

        [TestMethod()]
        public async Task UpdateDepartmentTestAsync()
        {
            var dept = await _service.GetDepartmentById(DepartmentID_1);
            dept.Name = "Sales & Aftersales";

            var updatedDept = await _service.UpdateDepartment(dept.Id, dept);
            Assert.AreEqual("Sales & Aftersales", updatedDept.Name);
        }
    }
}