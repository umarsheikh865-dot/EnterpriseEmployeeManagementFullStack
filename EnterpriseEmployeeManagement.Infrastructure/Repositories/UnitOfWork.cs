using EnterpriseEmployeeManagement.Application.Interfaces.Repositories;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;

namespace EnterpriseEmployeeManagement.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IEmployeeRepository Employees { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IEmployeeRepository employeeRepository)
        {
            _context = context;
            Employees = employeeRepository;
        }

        public IGenericRepository<TEntity> Repository<TEntity>()
            where TEntity : class
        {
            return new GenericRepository<TEntity>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}