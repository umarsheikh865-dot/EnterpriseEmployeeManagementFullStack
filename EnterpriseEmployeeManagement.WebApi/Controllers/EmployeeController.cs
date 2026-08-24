using EnterpriseEmployeeManagement.Application.DTOs;
using EnterpriseEmployeeManagement.Application.DTOs.Employees;
using EnterpriseEmployeeManagement.Application.Interfaces;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseEmployeeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();

            return Ok(employees);
        }

        // GET: api/Employee/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        // GET: api/Employee/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedEmployeeResponseDto>> GetPaged(
            [FromQuery] EmployeeQueryDto query)
        {
            var result = await _employeeService.GetPagedAsync(query);

            return Ok(result);
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<ActionResult<EmployeeResponseDto>> Create(
            CreateEmployeeDto dto)
        {
            var employee = await _employeeService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = employee.Id },
                employee);
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> Update(
            int id,
            UpdateEmployeeDto dto)
        {
            var employee = await _employeeService.UpdateAsync(id, dto);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _employeeService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}