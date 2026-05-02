using ChargePoint.CarManagement.Application.Interfaces.TireService;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Models;
using System.ComponentModel.DataAnnotations;
using ZiggyCreatures.Caching.Fusion;

namespace ChargePoint.CarManagement.Infrastructure.Services
{
    public class TireDraftService(IFusionCache memoryCache) : ITireDraftService
    {
        private readonly IFusionCache _memoryCache = memoryCache;

        private string CacheKey(int carId, string? username)
            => $"MaintenanceCreate_{carId}_{username}";

        public ResolvedDraftResult? ResolveDraft(int carId, string? username)
        {
            var key = CacheKey(carId, username);

            // Thử lấy dạng mới (MaintenanceDraftCache)
            var draftCache = _memoryCache.TryGet<MaintenanceDraftCache>(key);
            if (draftCache.HasValue && draftCache.Value?.MaintenanceRecord != null)
            {
                return new ResolvedDraftResult(
                    draftCache.Value.MaintenanceRecord,
                    draftCache.Value.HinhAnhChungTuFiles ?? []);
            }

            // Fallback: dạng cũ (MaintenanceRecord trực tiếp)
            var legacy = _memoryCache.TryGet<MaintenanceRecord>(key);
            if (legacy.HasValue && legacy.Value != null)
            {
                return new ResolvedDraftResult(legacy.Value, []);
            }

            return null;
        }

        public void RemoveDraft(int carId, string? username)
            => _memoryCache.Remove(CacheKey(carId, username));

        public IList<string> ValidateDraft(MaintenanceRecord draft)
        {
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(draft);
            Validator.TryValidateObject(draft, ctx, results, validateAllProperties: true);
            return results.Select(r => r.ErrorMessage ?? "Lỗi không xác định").ToList();
        }
    }
}