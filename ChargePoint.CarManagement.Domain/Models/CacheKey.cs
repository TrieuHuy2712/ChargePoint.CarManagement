namespace ChargePoint.CarManagement.Domain.Models
{
    public class CacheKey
    {
        public static string GetMaintenanceDraftCacheKey(int carId, string userName)
            => $"MaintenanceCreate_{carId}_{userName}";

        public static string GetMaintenanceCreateDraftCacheKey(int carId, string userName)
            => $"MaintenanceCreate_{carId}_{userName}";
        public static string GetMaintenanceEditDraftCacheKey(int carId, string userName)
            => $"MaintenanceEdit_{carId}_{userName}";

    }
}
