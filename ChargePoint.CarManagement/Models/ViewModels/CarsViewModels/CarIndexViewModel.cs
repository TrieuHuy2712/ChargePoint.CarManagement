namespace ChargePoint.CarManagement.Models.ViewModels.CarsViewModels
{
    public class CarIndexViewModel
    {
        public IEnumerable<Car> Cars { get; set; } = new List<Car>();

        // Tìm kiếm + phân trang
        public string? SearchQuery { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
