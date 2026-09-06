namespace EnterpriseEmployeeManagement.Application.DTOs.Common
{
    public class PaginationRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public string SortOrder { get; set; } = "asc";

        public int? DepartmentId { get; set; }
    }
}