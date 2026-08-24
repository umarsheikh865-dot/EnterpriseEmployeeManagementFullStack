using EnterpriseEmployeeManagement.Application.Interfaces.Repositories;
using EnterpriseEmployeeManagement.Domain.Entities;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagement.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // GET ALL EMPLOYEES
        // ============================================

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .ToListAsync();
        }

        // ============================================
        // GET EMPLOYEE BY ID
        // ============================================

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        // ============================================
        // GET EMPLOYEE BY EMAIL
        // ============================================

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        // ============================================
        // ADD EMPLOYEE
        // ============================================

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        // ============================================
        // UPDATE EMPLOYEE
        // ============================================

        public void Update(Employee employee)
        {
            _context.Employees.Update(employee);
        }

        // ============================================
        // DELETE EMPLOYEE
        // ============================================

        public void Delete(Employee employee)
        {
            _context.Employees.Remove(employee);
        }

        // ============================================
        // CHECK DEPARTMENT
        // ============================================

        public async Task<bool> DepartmentExistsAsync(int id)
        {
            return await _context.Departments
                .AnyAsync(d => d.Id == id);
        }

        // ============================================
        // CHECK ROLE
        // ============================================

        public async Task<bool> RoleExistsAsync(int id)
        {
            return await _context.Roles
                .AnyAsync(r => r.Id == id);
        }

        // ============================================
        // SAVE CHANGES
        // ============================================

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        // ============================================
        // PAGINATED EMPLOYEES
        // ============================================

        public async Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            int? departmentId,
            string? sortBy,
            string? sortOrder)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .AsQueryable();

            // ============================================
            // SEARCHING
            // ============================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.FirstName.Contains(search) ||
                    e.LastName.Contains(search) ||
                    e.Email.Contains(search));
            }

            // ============================================
            // FILTERING BY DEPARTMENT
            // ============================================

            if (departmentId.HasValue)
            {
                query = query.Where(e =>
                    e.DepartmentId == departmentId.Value);
            }

            // ============================================
            // TOTAL COUNT
            // ============================================

            var totalCount = await query.CountAsync();

            // ============================================
            // SORTING
            // ============================================

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.ToLower() == "firstname")
                {
                    if (sortOrder?.ToLower() == "desc")
                    {
                        query = query.OrderByDescending(e => e.FirstName);
                    }
                    else
                    {
                        query = query.OrderBy(e => e.FirstName);
                    }
                }
                else if (sortBy.ToLower() == "lastname")
                {
                    if (sortOrder?.ToLower() == "desc")
                    {
                        query = query.OrderByDescending(e => e.LastName);
                    }
                    else
                    {
                        query = query.OrderBy(e => e.LastName);
                    }
                }
                else if (sortBy.ToLower() == "email")
                {
                    if (sortOrder?.ToLower() == "desc")
                    {
                        query = query.OrderByDescending(e => e.Email);
                    }
                    else
                    {
                        query = query.OrderBy(e => e.Email);
                    }
                }
                else
                {
                    query = query.OrderBy(e => e.Id);
                }
            }
            else
            {
                query = query.OrderBy(e => e.Id);
            }

            // ============================================
            // PAGINATION
            // ============================================

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}