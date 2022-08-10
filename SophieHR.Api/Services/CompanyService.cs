using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.DAL;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;

namespace SophieHR.Api.Services
{
    public interface ICompanyService
    {
        Task<ICollection<CompanyDetailNoLogo>> GetAllCompaniesNoLogoAsync();

        Task<ICollection<KeyValuePair<Guid, string>>> GetCompanyNamesAsync(string username, bool isManager = false);

        Task<CompanyDetailDto> GetCompanyByIdNoTrackingAsync(Guid id);

        Task<Company> FindCompanyByIdAsync(Guid id);

        Task<HttpResponseMessage> UpdateCompanyAsync(Guid id, CompanyDetailNoLogo companyDetail);

        Task<HttpResponseMessage> UploadLogoForCompanyAsync(Guid id, IFormFile logo);

        Task<CompanyDetailDto> CreateNewCompanyAsync(CompanyCreateDto companyDto);

        Task<HttpResponseMessage> DeleteCompanyAsync(Guid companyId);
    }

    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;
        private readonly ILogger<CompanyService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CompanyService(ApplicationDbContext context, IMapper mapper, ILogger<CompanyService> logger, IUnitOfWork unitOfWork)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ICollection<CompanyDetailNoLogo>> GetAllCompaniesNoLogoAsync()
        {
            _logger.LogInformation($"{nameof(GetAllCompaniesNoLogoAsync)} called");
            
            return _mapper.Map<List<CompanyDetailNoLogo>>(await _unitOfWork.Companies.GetAllAsync());
        }

        public async Task<ICollection<KeyValuePair<Guid, string>>> GetCompanyNamesAsync(string username, bool isManager = false)
        {
            _logger.LogInformation($"{nameof(GetCompanyNamesAsync)} called");
            var companies = await _unitOfWork.Companies.GetCompanyNamesAsync();
            if (isManager)
            {
                var companyId = await _context.Employees.Where(x => x.UserName == username).Select(x => x.CompanyId).SingleOrDefaultAsync();
                companies = companies.Where(x => x.Key == companyId).ToList();
            }
            return companies;
        }

        public async Task<CompanyDetailDto> GetCompanyByIdNoTrackingAsync(Guid id)
        {
            _logger.LogInformation($"{nameof(GetCompanyByIdNoTrackingAsync)} called");

            return _unitOfWork.Companies.Find(c => c.Id == id, includeProperties: "Address,Employees").Select(x => new CompanyDetailDto
            {
                Address = x.Address,
                CreatedDate = x.CreatedDate,
                EmployeeCount = x.Employees.Count(),
                Id = x.Id,
                Logo = x.Logo != null ? Convert.ToBase64String(x.Logo) : null,
                Name = x.Name,
                UpdatedDate = x.UpdatedDate
            }).FirstOrDefault();

        }

        public async Task<Company> FindCompanyByIdAsync(Guid id)
        {
            _logger.LogInformation($"{nameof(FindCompanyByIdAsync)} called");
            return await _unitOfWork.Companies.GetByIdAsync(id);
            //return await _context.Companies.FindAsync(id);
        }

        public async Task<HttpResponseMessage> UpdateCompanyAsync(Guid id, CompanyDetailNoLogo companyDetail)
        {
            _logger.LogInformation($"{nameof(UpdateCompanyAsync)} called");
            if (companyDetail.Id != id)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent($"Id's do not match!") };
            }
            var originalCompany = await _unitOfWork.Companies.GetByIdAsync(id);
            if (originalCompany == null)
            {
                _logger.LogWarning($"{nameof(UpdateCompanyAsync)} Unable to find original company with id {id}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent($"Unable to find original company with id {id}") };
            }
            var company = _mapper.Map(companyDetail, originalCompany);
            _unitOfWork.Companies.Update(company);
            _unitOfWork.Complete();
            return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
        }

        public async Task<HttpResponseMessage> UploadLogoForCompanyAsync(Guid id, IFormFile logo)
        {
            _logger.LogInformation($"{nameof(UploadLogoForCompanyAsync)} called");
            var result = new HttpResponseMessage();
            if (logo != null)
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    _logger.LogWarning($"{nameof(UpdateCompanyAsync)} Unable to find company with id {id}");
                    //return NotFound($"Unable to find a company with the Id of {id}");
                    result.StatusCode = System.Net.HttpStatusCode.NotFound;
                    result.Content = new StringContent($"Unable to find company with id {id}");
                    return result;
                }

                // Resize the image to be 256 * 256...
                using (var memoryStream = new MemoryStream())
                {
                    await logo.CopyToAsync(memoryStream);
                    byte[] bytes = memoryStream.ToArray();

                    company.Logo = bytes;
                    await _context.SaveChangesAsync();
                }
            }
            result.StatusCode = System.Net.HttpStatusCode.NoContent;
            result.Content = new StringContent("Sucessfully added logo");
            return result;
        }

        public async Task<CompanyDetailDto> CreateNewCompanyAsync(CompanyCreateDto companyDto)
        {
            _logger.LogInformation($"{nameof(CreateNewCompanyAsync)} called");
            var company = _mapper.Map<Company>(companyDto);
            _context.Companies.Add(company);
            try
            {
                await _context.SaveChangesAsync();
                return _mapper.Map<CompanyDetailDto>(company);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteCompanyAsync(Guid companyId)
        {
            _logger.LogInformation($"{nameof(DeleteCompanyAsync)} called");
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
            {
                _logger.LogWarning($"Unable to find company with id {companyId}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"Unable to find company with id {companyId}") };
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
        }
    }
}