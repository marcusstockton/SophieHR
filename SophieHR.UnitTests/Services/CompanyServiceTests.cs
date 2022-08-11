using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;
using SophieHR.Api.Profiles;
using System.Net;
using System.Text;

namespace SophieHR.Api.Services.Tests
{
    [TestClass()]
    public class CompanyServiceTests
    {
        private ApplicationDbContext _context;
        private CompanyService _service;

        private Guid _id1;
        private Guid _id2;

        [TestInitialize()]
        public async Task SetupDataAsync()
        {
            var _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nameof(CompanyServiceTests))
                .Options;

            _context = new ApplicationDbContext(_options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CompanyProfile());
                cfg.AddProfile(new AddressProfile());
            });
            var mapper = config.CreateMapper();

            var mockLogger = new Mock<ILogger<CompanyService>>();

            _id1 = Guid.NewGuid();
            _id2 = Guid.NewGuid();
            var companyList = new List<Company>
            {
                new Company{ Id = _id1, Name = "Test Company One", Address = new CompanyAddress{ Line1 = "Line 1", Line2 = "Line 2", Postcode = "EX11EX" } },
                new Company{ Id = _id2, Name = "Test Company Two", Address = new CompanyAddress{ Line1 = "Line 1", Line2 = "Line 2", Postcode = "EX22EX" } },
            };

            await _context.Companies.AddRangeAsync(companyList);
            await _context.SaveChangesAsync();

            _service = new CompanyService(_context, mapper, mockLogger.Object);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            _context.Dispose();
        }

        [TestMethod()]
        public async Task GetAllCompaniesNoLogoAsyncTestAsync()
        {
            var results = await _service.GetAllCompaniesNoLogoAsync();
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod()]
        public async Task GetCompanyNamesAsyncTest()
        {
            var results = await _service.GetCompanyNamesAsync("adminUser");
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod()]
        public async Task GetCompanyByIdNoTrackingAsyncTest()
        {
            var company = await _service.GetCompanyByIdNoTrackingAsync(_id1);
            Assert.AreEqual("Test Company One", company.Name);
        }

        [TestMethod()]
        public async Task FindCompanyByIdAsyncTestAsync()
        {
            var company = await _service.FindCompanyByIdAsync(_id2);
            Assert.AreEqual("Test Company Two", company.Name);
        }

        [TestMethod()]
        public async Task UpdateCompanyAsyncTestAsync()
        {
            var company = await _service.FindCompanyByIdAsync(_id2);

            var companyUpdate = new CompanyDetailNoLogo
            {
                Id = company.Id,
                Name = "Updated Company Name Two",
                Address = company.Address,
                CreatedDate = company.CreatedDate,
                UpdatedDate = company.UpdatedDate
            };

            var result = await _service.UpdateCompanyAsync(_id2, companyUpdate);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod()]
        public async Task UpdateCompanyWithInvalidIdsAsyncTestAsync()
        {
            var company = await _service.FindCompanyByIdAsync(_id2);

            var companyUpdate = new CompanyDetailNoLogo
            {
                Id = Guid.NewGuid(),
                Name = "Updated Company Name Two",
                Address = company.Address,
                CreatedDate = company.CreatedDate,
                UpdatedDate = company.UpdatedDate
            };

            var result = await _service.UpdateCompanyAsync(_id2, companyUpdate);
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.AreEqual("Id's do not match!", resultContent);
        }

        [TestMethod()]
        public async Task UpdateCompanyWithInvalidCompanyIdAsyncTestAsync()
        {
            var unknownId = Guid.NewGuid();
            var companyUpdate = new CompanyDetailNoLogo
            {
                Id = unknownId,
                Name = "This company doesn't exist",
                Address = new CompanyAddress { County = "Devon" },
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            var result = await _service.UpdateCompanyAsync(unknownId, companyUpdate);
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.AreEqual($"Unable to find original company with id {unknownId}", resultContent);
        }

        [TestMethod()]
        public async Task UploadLogoForCompanyAsyncTestAsync()
        {
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            var result = await _service.UploadLogoForCompanyAsync(_id1, file);

            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
            var contentResult = await result.Content.ReadAsStringAsync();
            Assert.AreEqual("Sucessfully added logo", contentResult);
        }

        [TestMethod()]
        public async Task UploadLogoForInvalidCompanyAsyncTestAsync()
        {
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            var badId = Guid.NewGuid();
            var result = await _service.UploadLogoForCompanyAsync(badId, file);

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            var contentResult = await result.Content.ReadAsStringAsync();
            Assert.AreEqual($"Unable to find company with id {badId}", contentResult);
        }

        [TestMethod()]
        public async Task CreateNewCompanyAsyncTestAsync()
        {
            var newCompany = new CompanyCreateDto
            {
                Name = "Company Three",
                Address = new Models.DTOs.Address.AddressCreateDto
                {
                    County = "Devon",
                    Line1 = "Line1",
                    Postcode = "ER11RE"
                }
            };

            var result = await _service.CreateNewCompanyAsync(newCompany);
            Assert.IsNotNull(result.Id);
        }

        [TestMethod()]
        public async Task DeleteCompanyAsyncTestAsync()
        {
            var result = await _service.DeleteCompanyAsync(_id2);

            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod()]
        public async Task DeleteCompanyAsyncTestFailsCorrectlyAsync()
        {
            var result = await _service.DeleteCompanyAsync(Guid.NewGuid());

            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }


        
    }
}