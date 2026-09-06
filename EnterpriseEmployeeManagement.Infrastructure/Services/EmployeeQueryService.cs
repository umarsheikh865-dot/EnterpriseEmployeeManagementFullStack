using EnterpriseEmployeeManagement.Application.DTOs;
using EnterpriseEmployeeManagement.Application.DTOs.Common;
using EnterpriseEmployeeManagement.Application.DTOs.Employees;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagement.Infrastructure.Services
{
    public class EmployeeQueryService : IEmployeeQueryService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<EmployeeResponseDto>> GetEmployeesAsync(
            EmployeeQueryDto request)
        {
            var pageNumber = request.PageNumber < 1
                ? 1
                : request.PageNumber;

            var pageSize = request.PageSize < 1
                ? 10
                : Math.Min(request.PageSize, 100);

            IQueryable<Domain.Entities.Employee> query =
                _context.Employees.AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(e =>
                    e.FirstName.Contains(search) ||
                    e.LastName.Contains(search) ||
                    e.Email.Contains(search));
            }

            // Department filter
            if (request.DepartmentId.HasValue)
            {
                query = query.Where(e =>
                    e.DepartmentId == request.DepartmentId.Value);
            }

            // Sorting
            var sortBy = request.SortBy?.ToLower() ?? "id";
            var sortOrder = request.SortOrder?.ToLower() ?? "asc";

            query = sortBy switch
            {
                "firstname" =>
                    sortOrder == "desc"
                        ? query.OrderByDescending(e => e.FirstName)
                        : query.OrderBy(e => e.FirstName),

                "lastname" =>
                    sortOrder == "desc"
                        ? query.OrderByDescending(e => e.LastName)
                        : query.OrderBy(e => e.LastName),

                "email" =>
                    sortOrder == "desc"
                        ? query.OrderByDescending(e => e.Email)
                        : query.OrderBy(e => e.Email),

                _ =>
                    query.OrderBy(e => e.Id)
            };

            // Total records
            var totalCount = await query.CountAsync();

            // Pagination + projection
            var employees = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    DepartmentId = e.DepartmentId,
                    RoleId = e.RoleId
                })
                .ToListAsync();

            // Response
            return new PagedResponse<EmployeeResponseDto>
            {
                Items = employees,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}