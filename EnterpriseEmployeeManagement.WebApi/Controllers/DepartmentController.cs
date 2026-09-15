using EnterpriseEmployeeManagement.Domain.Entities;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/departments")] // Changed to plural 'departments' to match frontend fetch
    //[Authorize] // Commented out for now to prevent 401 errors if frontend token isn't passed here yet
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/departments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _context.Set<Department>()
                .AsNoTracking()
                .ToListAsync();

            return Ok(departments);
        }

        // GET: api/departments/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await _context.Set<Department>()
                .FindAsync(id);

            if (department == null)
            {
                return NotFound(new
                {
                    message = "Department not found."
                });
            }

            return Ok(department);
        }

        // POST: api/departments
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Department department)
        {
            department.Id = 0;

            _context.Set<Department>().Add(department);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = department.Id },
                department);
        }

        // PUT: api/departments/1
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            Department department)
        {
            if (id != department.Id)
            {
                return BadRequest(new
                {
                    message = "ID mismatch."
                });
            }

            var existingDepartment =
                await _context.Set<Department>()
                    .FindAsync(id);

            if (existingDepartment == null)
            {
                return NotFound(new
                {
                    message = "Department not found."
                });
            }

            _context.Entry(existingDepartment).CurrentValues
                .SetValues(department);

            await _context.SaveChangesAsync();

            return Ok(existingDepartment);
        }

        // DELETE: api/departments/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var department =
                await _context.Set<Department>()
                    .FindAsync(id);

            if (department == null)
            {
                return NotFound(new
                {
                    message = "Department not found."
                });
            }

            _context.Set<Department>().Remove(department);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Department deleted successfully."
            });
        }
    }
}