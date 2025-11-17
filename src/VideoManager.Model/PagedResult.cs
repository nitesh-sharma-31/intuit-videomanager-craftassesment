namespace VideoManager.Model
{
    /// <summary>
    /// Represents a paginated result set
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Query parameters for pagination and filtering
    /// </summary>
    public class QueryParameters
    {
        private int _pageSize = 20;
        private const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string SearchTerm { get; set; } = string.Empty;
        public string SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
        public string Filter { get; set; } = string.Empty;
    }
}
