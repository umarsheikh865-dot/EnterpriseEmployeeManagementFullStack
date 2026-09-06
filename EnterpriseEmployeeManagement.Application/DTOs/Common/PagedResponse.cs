namespace EnterpriseEmployeeManagement.Application.DTOs.Common
{
    public class PagedResponse<T>
    {
        public IReadOnlyList<T> Items { get; set; }
            = new List<T>();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(
                TotalCount / (double)PageSize);

        public bool HasNextPage =>
            PageNumber < TotalPages;

        public bool HasPreviousPage =>
            PageNumber > 1;
    }
}