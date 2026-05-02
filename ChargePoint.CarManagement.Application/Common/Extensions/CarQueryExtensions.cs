using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Application.Common.Extensions
{
    /// <summary>
    /// Extension methods for IQueryable<Car> to provide reusable search/filter functionality
    /// </summary>
    public static class CarQueryExtensions
    {
        /// <summary>
        /// Apply search filter to Cars query using optimized MySQL LIKE
        /// Searches across: BienSo, TenXe, TenKhachHang, SoVIN, MauXe
        /// </summary>
        /// <param name="query">The IQueryable<Car> to filter</param>
        /// <param name="searchTerm">Search term (case-insensitive)</param>
        /// <returns>Filtered IQueryable<Car></returns>
        public static IQueryable<Domain.Entities.Car> ApplySearch(this IQueryable<Domain.Entities.Car> query, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return query;

            var searchKey = searchTerm.Trim();
            var searchKeyNormalized = searchKey.Replace("-", "").Replace(".", "");

            // MySQL with utf8mb4_unicode_ci collation is case-insensitive by default
            // Use EF.Functions.Like for better MySQL optimization with indexes
            return query.Where(c =>
                (c.BienSo != null && (
                    EF.Functions.Like(c.BienSo, $"%{searchKey}%") ||
                    EF.Functions.Like(c.BienSo.Replace("-", "").Replace(".", ""), $"%{searchKeyNormalized}%")
                )) ||
                (c.TenXe != null && EF.Functions.Like(c.TenXe, $"%{searchKey}%")) ||
                (c.TenKhachHang != null && EF.Functions.Like(c.TenKhachHang, $"%{searchKey}%")) ||
                (c.SoVIN != null && EF.Functions.Like(c.SoVIN, $"%{searchKey}%")) ||
                (c.MauXe != null && EF.Functions.Like(c.MauXe, $"%{searchKey}%"))
            );
        }

        /// <summary>
        /// Apply default ordering by Stt (car sequence number)
        /// </summary>
        /// <param name="query">The IQueryable<Car> to order</param>
        /// <returns>Ordered IQueryable<Car></returns>
        public static IQueryable<Domain.Entities.Car> ApplyDefaultOrdering(this IQueryable<Domain.Entities.Car> query)
        {
            return query.OrderBy(c => c.Stt);
        }

        /// <summary>
        /// Apply pagination to Cars query
        /// </summary>
        /// <param name="query">The IQueryable<Car> to paginate</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated IQueryable<Car></returns>
        public static IQueryable<Domain.Entities.Car> ApplyPagination(this IQueryable<Domain.Entities.Car> query, int pageNumber, int pageSize)
        {
            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>
        /// Apply search, ordering, and pagination in one call
        /// </summary>
        /// <param name="query">The IQueryable<Car> to filter</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Filtered, ordered, and paginated IQueryable<Car></returns>
        public static IQueryable<Domain.Entities.Car> ApplySearchAndPagination(
            this IQueryable<Domain.Entities.Car> query,
            string? searchTerm,
            int pageNumber,
            int pageSize)
        {
            return query
                .ApplySearch(searchTerm)
                .ApplyDefaultOrdering()
                .ApplyPagination(pageNumber, pageSize);
        }
    }
}
