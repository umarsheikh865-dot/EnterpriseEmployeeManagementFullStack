using EnterpriseEmployeeManagement.Domain.Entities;

namespace EnterpriseEmployeeManagement.Application.Interfaces.Repositories
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<Employee?> GetByEmailAsync(string email);

        Task<bool> DepartmentExistsAsync(int id);

        Task<bool> RoleExistsAsync(int id);

        Task<bool> SaveChangesAsync();

        Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            int? departmentId,
            string? sortBy,
            string? sortOrder);
    }
}