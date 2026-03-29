namespace ChargePoint.CarManagement.Models.ViewModels
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public string? SearchQuery { get; set; }

        public bool Any()
        {
            return Items != null && Enumerable.Any(Items);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items?.GetEnumerator() ?? Enumerable.Empty<T>().GetEnumerator();
        }
    }
}
