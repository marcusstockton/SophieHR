using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Services;
using System.Security.Claims;

namespace SophieHR.UnitTests.Services
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private MockRepository mockRepository;

        private Mock<ApplicationDbContext> mockApplicationDbContext;
        private Mock<UserManager<ApplicationUser>> mockUserManager;
        private Mock<IMapper> mockMapper;
        private Mock<ILogger<EmployeeService>> mockLogger;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            var _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nameof(EmployeeServiceTests))
                .Options;

            this.mockApplicationDbContext = this.mockRepository.Create<ApplicationDbContext>();
            this.mockUserManager = this.mockRepository.Create<UserManager<ApplicationUser>>();
            this.mockMapper = this.mockRepository.Create<IMapper>();
            this.mockLogger = this.mockRepository.Create<ILogger<EmployeeService>>();
        }

        private EmployeeService CreateService()
        {
            return new EmployeeService(
                this.mockApplicationDbContext.Object,
                this.mockUserManager.Object,
                this.mockMapper.Object,
                this.mockLogger.Object);
        }

        [TestMethod]
        public async Task CreateEmployee_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            EmployeeCreateDto employeeDto = null;
            EmployeeDetailDto manager = null;
            string role = null;

            // Act
            var result = await service.CreateEmployee(
                employeeDto,
                manager,
                role);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEmployee_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            Guid employeeId = default(global::System.Guid);

            // Act
            await service.DeleteEmployee(
                employeeId);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task GetEmployeeById_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            Guid employeeId = default(global::System.Guid);
            ClaimsPrincipal user = null;

            // Act
            var result = await service.GetEmployeeById(
                employeeId,
                user);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task GetEmployeeByUsername_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            string username = null;

            // Act
            var result = await service.GetEmployeeByUsername(
                username);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task GetEmployeesForCompanyId_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            Guid companyId = default(global::System.Guid);

            // Act
            var result = await service.GetEmployeesForCompanyId(
                companyId);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task GetEmployeesForManager_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            Guid managerId = default(global::System.Guid);

            // Act
            var result = await service.GetEmployeesForManager(
                managerId);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task GetManagersForCompanyId_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            Guid companyId = default(global::System.Guid);

            // Act
            var result = await service.GetManagersForCompanyId(
                companyId);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetTitles_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();

            // Act
            var result = service.GetTitles();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task UpdateEmployee_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            EmployeeDetailDto employeeDto = null;

            // Act
            var result = await service.UpdateEmployee(
                employeeDto);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task UploadAvatarToEmployee_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            Guid id = default(global::System.Guid);
            IFormFile avatar = null;

            // Act
            await service.UploadAvatarToEmployee(
                id,
                avatar);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
