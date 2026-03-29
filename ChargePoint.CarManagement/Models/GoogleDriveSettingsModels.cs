namespace ChargePoint.CarManagement.Models
{
    public class GoogleDriveSettingsModel
    {
        public string ServiceAccountEmail { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = "ChargePoint Car Management";
        public string RootFolderId { get; set; } = string.Empty;
    }
}
