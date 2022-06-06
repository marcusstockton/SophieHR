﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        Task<HttpResponseMessage> CreateNewCompanyAsync(CompanyCreateDto companyDto);
        Task<HttpResponseMessage> DeleteCompanyAsync(Guid companyId);
    }

    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(ApplicationDbContext context, IMapper mapper, ILogger<CompanyService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ICollection<CompanyDetailNoLogo>> GetAllCompaniesNoLogoAsync()
        {
            _logger.LogInformation($"{nameof(CompanyService)} calling {GetAllCompaniesNoLogoAsync}");
            return _mapper.Map<List<CompanyDetailNoLogo>>(await _context.Companies.ToListAsync());
        }

        public async Task<ICollection<KeyValuePair<Guid, string>>> GetCompanyNamesAsync(string username, bool isManager = false)
        {
            _logger.LogInformation($"{nameof(CompanyService)} calling {nameof(GetCompanyNamesAsync)}");
            var companies = await _context.Companies.Select(x => new KeyValuePair<Guid, string>(x.Id, x.Name)).ToListAsync();
            if (isManager)
            {
                var companyId = await _context.Employees.Where(x => x.UserName == username).Select(x => x.CompanyId).SingleOrDefaultAsync();
                companies = companies.Where(x => x.Key == companyId).ToList();
            }
            return companies;
        }

        public async Task<CompanyDetailDto> GetCompanyByIdNoTrackingAsync(Guid id)
        {
            _logger.LogInformation($"{nameof(CompanyService)} calling {nameof(GetCompanyByIdNoTrackingAsync)}");

            return await _context.Companies
                .Include(x => x.Address)
                .Include(x => x.Employees)
                .AsNoTracking()
                .Select(x => new CompanyDetailDto
                {
                    Address = x.Address,
                    CreatedDate = x.CreatedDate,
                    EmployeeCount = x.Employees.Count(),
                    Id = x.Id,
                    Logo = x.Logo != null ? Convert.ToBase64String(x.Logo) : null,
                    Name = x.Name,
                    UpdatedDate = x.UpdatedDate
                })
                .FirstAsync(x => x.Id == id);
        }

        public async Task<Company> FindCompanyByIdAsync(Guid id)
        {
            return await _context.Companies.FindAsync(id);
        }

        public async Task<HttpResponseMessage> UpdateCompanyAsync(Guid id, CompanyDetailNoLogo companyDetail)
        {
            var originalCompany = await _context.Companies.FindAsync(id);
            if (originalCompany == null)
            {
                _logger.LogWarning($"Unable to find original company with id {id}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent($"Unable to find original company with id {id}") };
            }
            var company = _mapper.Map(companyDetail, originalCompany);
            _context.Companies.Attach(company);
            _context.Entry(company).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
        }

        public async Task<HttpResponseMessage> UploadLogoForCompanyAsync(Guid id, IFormFile logo)
        {
            var result = new HttpResponseMessage();
            if (logo != null)
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    _logger.LogWarning($"Unable to find company with id {id}");
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

        public async Task<HttpResponseMessage> CreateNewCompanyAsync(CompanyCreateDto companyDto)
        {
            var company = _mapper.Map<Company>(companyDto);
            _context.Companies.Add(company);
            try
            {
                await _context.SaveChangesAsync();
                return new HttpResponseMessage(System.Net.HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) };
            }
        }

        public async Task<HttpResponseMessage> DeleteCompanyAsync(Guid companyId)
        {
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
