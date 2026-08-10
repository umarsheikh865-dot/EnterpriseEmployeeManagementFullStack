using EnterpriseEmployeeManagement.Domain.Entities;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Role
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _context.Set<Role>()
                .AsNoTracking()
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/Role/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _context.Set<Role>()
                .FindAsync(id);

            if (role == null)
            {
                return NotFound(new
                {
                    message = "Role not found."
                });
            }

            return Ok(role);
        }

        // POST: api/Role
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            role.Id = 0;

            _context.Set<Role>().Add(role);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = role.Id },
                role);
        }

        // PUT: api/Role/1
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            Role role)
        {
            if (id != role.Id)
            {
                return BadRequest(new
                {
                    message = "ID mismatch."
                });
            }

            var existingRole =
                await _context.Set<Role>()
                    .FindAsync(id);

            if (existingRole == null)
            {
                return NotFound(new
                {
                    message = "Role not found."
                });
            }

            _context.Entry(existingRole).CurrentValues
                .SetValues(role);

            await _context.SaveChangesAsync();

            return Ok(existingRole);
        }

        // DELETE: api/Role/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role =
                await _context.Set<Role>()
                    .FindAsync(id);

            if (role == null)
            {
                return NotFound(new
                {
                    message = "Role not found."
                });
            }

            _context.Set<Role>().Remove(role);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Role deleted successfully."
            });
        }
    }
}