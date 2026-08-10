namespace EnterpriseEmployeeManagement.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<TEntity> Repository<TEntity>()
            where TEntity : class;

        IEmployeeRepository Employees { get; }

        Task<int> SaveChangesAsync();
    }
}