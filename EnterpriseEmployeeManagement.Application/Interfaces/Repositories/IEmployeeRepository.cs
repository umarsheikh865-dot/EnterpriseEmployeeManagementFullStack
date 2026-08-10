using EnterpriseEmployeeManagement.Domain.Entities;

namespace EnterpriseEmployeeManagement.Application.Interfaces.Repositories
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<Employee?> GetByEmailAsync(string email);
    }
}