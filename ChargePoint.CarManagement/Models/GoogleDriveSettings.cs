namespace ChargePoint.CarManagement.Models
{
    public class GoogleDriveSettings
    {
        public string ServiceAccountEmail { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;       // ✅ Mới
        public string ClientSecret { get; set; } = string.Empty;   // ✅ Mới  
        public string RefreshToken { get; set; } = string.Empty;   // ✅ Mới
        public string ApplicationName { get; set; } = "ChargePoint Car Management";
        public string RootFolderId { get; set; } = string.Empty;
    }
}
