using EnterpriseEmployeeManagement.Application.DTOs.Employees;
using EnterpriseEmployeeManagement.Application.Interfaces;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();

            return Ok(employees);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
                return NotFound(new
                {
                    message = "Employee not found."
                });

            return Ok(employee);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            CreateEmployeeDto dto)
        {
            try
            {
                var employee =
                    await _employeeService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = employee.Id },
                    employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            int id,
            UpdateEmployeeDto dto)
        {
            try
            {
                var updated =
                    await _employeeService.UpdateAsync(id, dto);

                if (updated == null)
                    return NotFound(new
                    {
                        message = "Employee not found."
                    });

                return Ok(new
                {
                    message = "Employee updated successfully.",
                    data = updated
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted =
                await _employeeService.DeleteAsync(id);

            if (!deleted)
                return NotFound(new
                {
                    message = "Employee not found."
                });

            return Ok(new
            {
                message = "Employee deleted successfully."
            });
        }
    }
}