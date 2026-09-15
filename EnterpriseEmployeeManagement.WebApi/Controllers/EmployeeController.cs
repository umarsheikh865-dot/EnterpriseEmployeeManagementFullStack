using EnterpriseEmployeeManagement.Application.DTOs;
using EnterpriseEmployeeManagement.Application.DTOs.Common;
using EnterpriseEmployeeManagement.Application.DTOs.Employees;
using EnterpriseEmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseEmployeeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/employees")]
    //[Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(
            IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // ============================================================
        // GET ALL EMPLOYEES
        // GET: api/Employee
        // ============================================================

        [HttpGet]
        public async Task<
            ActionResult<IEnumerable<EmployeeResponseDto>>>
            GetAll()
        {
            var employees =
                await _employeeService.GetAllAsync();

            return Ok(employees);
        }

        // ============================================================
        // GET EMPLOYEE BY ID
        // GET: api/Employee/5
        // ============================================================

        [HttpGet("{id}")]
        public async Task<
            ActionResult<EmployeeResponseDto>>
            GetById(int id)
        {
            var employee =
                await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        // ============================================================
        // GET PAGINATED EMPLOYEES
        // GET: api/Employee/paged
        //
        // Example:
        // /api/Employee/paged?pageNumber=1&pageSize=10
        // ============================================================

        [HttpGet("paged")]
        public async Task<
            ActionResult<PagedResponse<EmployeeResponseDto>>>
            GetPaged(
                [FromQuery] EmployeeQueryDto query)
        {
            var result =
                await _employeeService.GetPagedAsync(query);

            return Ok(result);
        }

        // ============================================================
        // SEARCH / FILTER EMPLOYEES
        // GET: api/Employee/search
        //
        // This uses the same EmployeeQueryDto and
        // IEmployeeService.
        // ============================================================

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] EmployeeQueryDto query)
        {
            var result =
                await _employeeService.GetPagedAsync(query);

            return Ok(result);
        }

        // ============================================================
        // CREATE EMPLOYEE
        // POST: api/Employee
        // ============================================================

        [HttpPost]
        public async Task<
            ActionResult<EmployeeResponseDto>>
            Create(
                CreateEmployeeDto dto)
        {
            var employee =
                await _employeeService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = employee.Id },
                employee);
        }

        // ============================================================
        // UPDATE EMPLOYEE
        // PUT: api/Employee/5
        // ============================================================

        [HttpPut("{id}")]
        public async Task<
            ActionResult<EmployeeResponseDto>>
            Update(
                int id,
                UpdateEmployeeDto dto)
        {
            var employee =
                await _employeeService.UpdateAsync(
                    id,
                    dto);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        // ============================================================
        // DELETE EMPLOYEE
        // DELETE: api/Employee/5
        // ============================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            var deleted =
                await _employeeService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}