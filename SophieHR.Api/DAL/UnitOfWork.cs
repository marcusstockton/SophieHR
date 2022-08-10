using SophieHR.Api.Data;
using SophieHR.Api.Models;

namespace SophieHR.Api.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        ICompanyRepository Companies { get; }
        //IProjectRepository Projects { get; }
        int Complete();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Companies = new CompanyRepository(_context);
            //Projects = new ProjectRepository(_context);
        }
        public ICompanyRepository Companies { get; private set; }
        //public IProjectRepository Projects { get; private set; }
        public int Complete()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
