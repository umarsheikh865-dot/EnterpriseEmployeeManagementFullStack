using EnterpriseEmployeeManagement.Application.DTOs.Employees;
using EnterpriseEmployeeManagement.Application.Interfaces;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Domain.Entities;
using EnterpriseEmployeeManagement.Infrastructure.Repositories;

namespace EnterpriseEmployeeManagement.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly EmployeeRepository _repository;
        private readonly IPasswordService _passwordService;

        public EmployeeService(
            EmployeeRepository repository,
            IPasswordService passwordService)
        {
            _repository = repository;
            _passwordService = passwordService;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync()
        {
            var employees = await _repository.GetAllAsync();

            return employees.Select(MapToResponse);
        }

        public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);

            return employee == null ? null : MapToResponse(employee);
        }

        public async Task<EmployeeResponseDto> CreateAsync(
            CreateEmployeeDto dto)
        {
            var existingEmployee =
                await _repository.GetByEmailAsync(dto.Email);

            if (existingEmployee != null)
            {
                throw new InvalidOperationException(
                    "An employee with this email already exists.");
            }

            if (!await _repository.DepartmentExistsAsync(dto.DepartmentId))
            {
                throw new InvalidOperationException(
                    "The selected department does not exist.");
            }

            if (!await _repository.RoleExistsAsync(dto.RoleId))
            {
                throw new InvalidOperationException(
                    "The selected role does not exist.");
            }

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                Salary = dto.Salary,
                DepartmentId = dto.DepartmentId,
                RoleId = dto.RoleId
            };

            await _repository.AddAsync(employee);
            await _repository.SaveChangesAsync();

            var createdEmployee =
                await _repository.GetByIdAsync(employee.Id);

            return MapToResponse(createdEmployee!);
        }

        public async Task<EmployeeResponseDto?> UpdateAsync(
            int id,
            UpdateEmployeeDto dto)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee == null)
                return null;

            var existingEmployee =
                await _repository.GetByEmailAsync(dto.Email);

            if (existingEmployee != null &&
                existingEmployee.Id != id)
            {
                throw new InvalidOperationException(
                    "An employee with this email already exists.");
            }

            if (!await _repository.DepartmentExistsAsync(dto.DepartmentId))
            {
                throw new InvalidOperationException(
                    "The selected department does not exist.");
            }

            if (!await _repository.RoleExistsAsync(dto.RoleId))
            {
                throw new InvalidOperationException(
                    "The selected role does not exist.");
            }

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.Salary = dto.Salary;
            employee.DepartmentId = dto.DepartmentId;
            employee.RoleId = dto.RoleId;

            _repository.Update(employee);

            await _repository.SaveChangesAsync();

            return MapToResponse(employee);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee == null)
                return false;

            _repository.Delete(employee);

            return await _repository.SaveChangesAsync();
        }

        private static EmployeeResponseDto MapToResponse(
            Employee employee)
        {
            return new EmployeeResponseDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Salary = employee.Salary,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.Name ?? string.Empty,
                RoleId = employee.RoleId,
                RoleName = employee.Role?.Name ?? string.Empty
            };
        }
    }
}