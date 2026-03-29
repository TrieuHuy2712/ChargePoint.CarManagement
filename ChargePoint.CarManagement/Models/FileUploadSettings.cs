namespace ChargePoint.CarManagement.Models
{
    public class FileUploadSettings
    {
        public int MaxImageSizeMB { get; set; } = 10;
        public int MaxVideoSizeMB { get; set; } = 100;
        public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        public string[] AllowedVideoExtensions { get; set; } = [".mp4", ".mov", ".avi", ".webm", ".mkv"];

        public long MaxImageSizeBytes => MaxImageSizeMB * 1024L * 1024L;
        public long MaxVideoSizeBytes => MaxVideoSizeMB * 1024L * 1024L;
    }
}
