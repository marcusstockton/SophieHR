using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;

namespace SophieHR.Api.DAL
{
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        Task<List<KeyValuePair<Guid, string>>> GetCompanyNamesAsync();
    }
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<KeyValuePair<Guid, string>>> GetCompanyNamesAsync()
        {
            return await _context.Companies.Select(x => new KeyValuePair<Guid, string>(x.Id, x.Name)).ToListAsync();
        }
    }
}
