using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Profiles;
using SophieHR.Api.Services;
using SophieHR.Api.Services.Tests;
using System.Security.Claims;

namespace SophieHR.UnitTests.Services
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private ApplicationDbContext _context;
        private EmployeeService _service;

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
                cfg.AddProfile(new AddressProfile());
            });
            var mapper = config.CreateMapper();

            var mockLogger = new Mock<ILogger<EmployeeService>>();

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

            var UserStoreMock = Mock.Of<IUserStore<ApplicationUser>>();
            var userMgr = new Mock<UserManager<ApplicationUser>>(UserStoreMock, null, null, null, null, null, null, null, null);

            _service = new EmployeeService(_context, userMgr.Object, mapper, mockLogger.Object);
        }

        [TestMethod]
        public async Task CreateEmployee_ThrowsException_When_Existing_Username_Passed_In()
        {
            // Arrange
            EmployeeCreateDto employeeDto = new EmployeeCreateDto { FirstName = "Damien", LastName = "Rice", WorkEmailAddress = "test@test.com"};
            EmployeeDetailDto manager = null;
            string role = null;

            // Act
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            {
                return _service.CreateEmployee(employeeDto, manager, role);
            });
        }

        [TestMethod]
        public async Task CreateEmployee_Creates_New_Employee_When_Valid_Username_Passed_In()
        {
            // Arrange
            EmployeeCreateDto employeeDto = new EmployeeCreateDto { FirstName = "Damien", LastName = "Rice", WorkEmailAddress = "test2@test.com" };
            EmployeeDetailDto manager = null;
            string role = null;

            // Act
            var result = await _service.CreateEmployee(employeeDto, manager, role);

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
        public async Task GetEmployeeById_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            Guid employeeId = _employeeId1;
            ClaimsPrincipal user = null;

            // Act
            var result = await _service.GetEmployeeById(
                employeeId,
                user);

            // Assert
            Assert.Fail();
        }

        //[TestMethod]
        //public async Task GetEmployeeByUsername_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    string username = null;

        //    // Act
        //    var result = await service.GetEmployeeByUsername(
        //        username);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task GetEmployeesForCompanyId_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    Guid companyId = default(global::System.Guid);

        //    // Act
        //    var result = await service.GetEmployeesForCompanyId(
        //        companyId);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task GetEmployeesForManager_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    Guid managerId = default(global::System.Guid);

        //    // Act
        //    var result = await service.GetEmployeesForManager(
        //        managerId);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task GetManagersForCompanyId_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    Guid companyId = default(global::System.Guid);

        //    // Act
        //    var result = await service.GetManagersForCompanyId(
        //        companyId);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void GetTitles_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();

        //    // Act
        //    var result = service.GetTitles();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task UpdateEmployee_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    EmployeeDetailDto employeeDto = null;

        //    // Act
        //    var result = await service.UpdateEmployee(
        //        employeeDto);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task UploadAvatarToEmployee_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    Guid id = default(global::System.Guid);
        //    IFormFile avatar = null;

        //    // Act
        //    await service.UploadAvatarToEmployee(
        //        id,
        //        avatar);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}
    }
}
