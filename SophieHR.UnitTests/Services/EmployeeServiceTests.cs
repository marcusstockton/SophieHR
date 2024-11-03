using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Profiles;
using SophieHR.Api.Services;
using System.Security.Claims;

namespace SophieHR.UnitTests.Services
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private ApplicationDbContext _context = default!;
        private EmployeeService _service = default!;
        private IMapper _mapper = default!;
        private Mock<ILogger<EmployeeService>> _loggerMock = new Mock<ILogger<EmployeeService>>();
        private Mock<UserManager<ApplicationUser>> _userManagerMock = new Mock<UserManager<ApplicationUser>>();
        private Mock<RoleManager<IdentityRole<Guid>>> _roleManagerMock = new Mock<RoleManager<IdentityRole<Guid>>>();

        private Guid _employeeId1 = Guid.NewGuid();
        private Guid _employeeId2 = Guid.NewGuid();
        private Guid _companyId1 = Guid.NewGuid();
        private Guid _companyId2 = Guid.NewGuid();

        [TestInitialize]
        public async Task TestInitialize()
        {
            var _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nameof(EmployeeServiceTests))
                .Options;

            _context = new ApplicationDbContext(_options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EmployeeProfile());
                cfg.AddProfile(new CompanyProfile());
                cfg.AddProfile(new AddressProfile());
            });
            _mapper = config.CreateMapper();

            var company1 = new Company { Id = _companyId1, Name = "Test Company One", Address = new CompanyAddress { Line1 = "Line 1", Line2 = "Line 2", Postcode = "EX11EX" } };
            var company2 = new Company { Id = _companyId2, Name = "Test Company Two", Address = new CompanyAddress { Line1 = "Line 1", Line2 = "Line 2", Postcode = "EX11EX" } };
            var companies = new List<Company> { company1, company2 };
            await _context.Companies.AddRangeAsync(companies);

            var employeeList = new List<Employee>
            {
                new Employee{ Id = _employeeId1, CompanyId = company1.Id, WorkEmailAddress = "test@test.com", Email = "test@test.com", Title = Title.Mr, JobTitle = "Guinea Pig Wrangler", FirstName = "Jim", LastName = "Kim", Gender = Gender.Male, UserName = "test@test.com", DateOfBirth = new DateTime(1992, 11, 15), StartOfEmployment=new DateTime(2022, 03, 21), Address = new EmployeeAddress{ Line1 = "Line 1", Line2 = "Line 2", Postcode = "EX11EX" } },
                new Employee{ Id = _employeeId2, CompanyId = company2.Id, WorkEmailAddress = "test1@test.com", Email = "test1@test.com", Title = Title.Miss, JobTitle = "Flea Circus Master", FirstName = "Kim", LastName = "Jim", Gender = Gender.Female, UserName = "test1@test.com", DateOfBirth = new DateTime(1979, 02, 21), StartOfEmployment=new DateTime(2020, 9, 7), Address = new EmployeeAddress{ Line1 = "Line 1", Line2 = "Line 2", Postcode = "EX22EX" } },
            };

            await _context.Employees.AddRangeAsync(employeeList);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

            var roleManagerMock = new Mock<RoleManager<IdentityRole<Guid>>>(
                new Mock<IRoleStore<IdentityRole<Guid>>>().Object,
                new IRoleValidator<IdentityRole<Guid>>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole<Guid>>>>().Object);

            _service = new EmployeeService(_context, userManagerMock.Object, roleManagerMock.Object, _mapper, _loggerMock.Object);
        }

        [TestMethod]
        public async Task CreateEmployee_ThrowsException_When_Existing_Username_Passed_In()
        {
            // Arrange
            var employeeDto = new EmployeeCreateDto { FirstName = "Damien", LastName = "Rice", WorkEmailAddress = "test@test.com" };

            // Act
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            {
                return _service.CreateEmployee(employeeDto);
            });
        }

        [TestMethod]
        public async Task CreateEmployee_Creates_New_Employee_When_Valid_Username_Passed_In()
        {
            // Arrange
            EmployeeCreateDto employeeDto = new EmployeeCreateDto { FirstName = "Damien", LastName = "Rice", WorkEmailAddress = "test2@test.com" };

            // Act
            var result = await _service.CreateEmployee(employeeDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Id != Guid.Empty);
        }

        [TestMethod]
        public async Task DeleteEmployee_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            Guid employeeId = _employeeId1;

            // Act
            var before = await _service.GetEmployeesForCompanyId(_companyId1);
            Assert.AreEqual(before.Count, 1);

            await _service.DeleteEmployee(employeeId);

            // Assert
            var after = await _service.GetEmployeesForCompanyId(_companyId1);
            Assert.AreEqual(after.Count, 0);
        }

        [TestMethod]
        public async Task GetEmployeeById_As_Manager_Returns_Correct_Employee()
        {
            // Arrange
            Guid employeeId = _employeeId1;

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);
            ClaimsPrincipal user = mockPrincipal.Object;

            // Act
            var result = await _service.GetEmployeeById(
                employeeId,
                user);

            // Assert
            Assert.AreEqual(result.UserName, "test@test.com");
        }

        [TestMethod]
        public async Task GetEmployeeById_As_User_Returns_User_Record()
        {
            // Arrange
            Guid employeeId = _employeeId1;

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(x => x.Identity.Name).Returns("test1@test.com");
            mockPrincipal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);
            ClaimsPrincipal user = mockPrincipal.Object;

            // Act
            var result = await _service.GetEmployeeById(
                employeeId,
                user);

            // Assert
            Assert.AreEqual(result.UserName, "test1@test.com");
        }
    }
}