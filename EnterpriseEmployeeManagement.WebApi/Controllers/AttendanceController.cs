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
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Attendance
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var attendance = await _context.Set<Attendance>()
                .AsNoTracking()
                .ToListAsync();

            return Ok(attendance);
        }

        // GET: api/Attendance/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var attendance =
                await _context.Set<Attendance>()
                    .FindAsync(id);

            if (attendance == null)
            {
                return NotFound(new
                {
                    message = "Attendance record not found."
                });
            }

            return Ok(attendance);
        }

        // POST: api/Attendance
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(
            Attendance attendance)
        {
            attendance.Id = 0;

            _context.Set<Attendance>().Add(attendance);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = attendance.Id },
                attendance);
        }

        // PUT: api/Attendance/1
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            Attendance attendance)
        {
            if (id != attendance.Id)
            {
                return BadRequest(new
                {
                    message = "ID mismatch."
                });
            }

            var existingAttendance =
                await _context.Set<Attendance>()
                    .FindAsync(id);

            if (existingAttendance == null)
            {
                return NotFound(new
                {
                    message = "Attendance record not found."
                });
            }

            _context.Entry(existingAttendance).CurrentValues
                .SetValues(attendance);

            await _context.SaveChangesAsync();

            return Ok(existingAttendance);
        }

        // DELETE: api/Attendance/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var attendance =
                await _context.Set<Attendance>()
                    .FindAsync(id);

            if (attendance == null)
            {
                return NotFound(new
                {
                    message = "Attendance record not found."
                });
            }

            _context.Set<Attendance>().Remove(attendance);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Attendance record deleted successfully."
            });
        }
    }
}