using EnterpriseEmployeeManagement.Application.DTOs;
using EnterpriseEmployeeManagement.Application.DTOs.Employees;

namespace EnterpriseEmployeeManagement.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> GetAllAsync();

        Task<EmployeeResponseDto?> GetByIdAsync(int id);

        Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto);

        Task<EmployeeResponseDto?> UpdateAsync(int id, UpdateEmployeeDto dto);

        Task<bool> DeleteAsync(int id);

        Task<PagedEmployeeResponseDto> GetPagedAsync(EmployeeQueryDto query);
    }
}