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
    }
}