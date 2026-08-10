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
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Leave
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var leaveRequests =
                await _context.Set<LeaveRequest>()
                    .AsNoTracking()
                    .ToListAsync();

            return Ok(leaveRequests);
        }

        // GET: api/Leave/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var leaveRequest =
                await _context.Set<LeaveRequest>()
                    .FindAsync(id);

            if (leaveRequest == null)
            {
                return NotFound(new
                {
                    message = "Leave request not found."
                });
            }

            return Ok(leaveRequest);
        }

        // POST: api/Leave
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(
            LeaveRequest leaveRequest)
        {
            leaveRequest.Id = 0;

            _context.Set<LeaveRequest>()
                .Add(leaveRequest);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = leaveRequest.Id },
                leaveRequest);
        }

        // PUT: api/Leave/1
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            LeaveRequest leaveRequest)
        {
            if (id != leaveRequest.Id)
            {
                return BadRequest(new
                {
                    message = "ID mismatch."
                });
            }

            var existingLeave =
                await _context.Set<LeaveRequest>()
                    .FindAsync(id);

            if (existingLeave == null)
            {
                return NotFound(new
                {
                    message = "Leave request not found."
                });
            }

            _context.Entry(existingLeave).CurrentValues
                .SetValues(leaveRequest);

            await _context.SaveChangesAsync();

            return Ok(existingLeave);
        }

        // DELETE: api/Leave/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var leaveRequest =
                await _context.Set<LeaveRequest>()
                    .FindAsync(id);

            if (leaveRequest == null)
            {
                return NotFound(new
                {
                    message = "Leave request not found."
                });
            }

            _context.Set<LeaveRequest>()
                .Remove(leaveRequest);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Leave request deleted successfully."
            });
        }
    }
}