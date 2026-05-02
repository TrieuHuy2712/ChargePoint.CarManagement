using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;

namespace ChargePoint.CarManagement.Application.Interfaces.TireService
{
    /// <summary>
    /// Xử lý luồng kết hợp Maintenance draft + Tire record.
    /// </summary>
    public interface ITireDraftService
    {
        /// <summary>
        /// Lấy MaintenanceDraftCache từ memory cache theo carId và user.
        /// Trả về null nếu không tìm thấy hoặc dữ liệu không hợp lệ.
        /// </summary>
        ResolvedDraftResult? ResolveDraft(int carId, string? username);

        /// <summary>
        /// Xóa draft khỏi cache.
        /// </summary>
        void RemoveDraft(int carId, string? username);

        /// <summary>
        /// Validate MaintenanceRecord draft.
        /// Trả về danh sách lỗi, rỗng nếu hợp lệ.
        /// </summary>
        IList<string> ValidateDraft(MaintenanceRecord draft);
    }

    public record ResolvedDraftResult(
        MaintenanceRecord MaintenanceRecord,
        List<CachedFileData> HinhAnhChungTuFiles);
}