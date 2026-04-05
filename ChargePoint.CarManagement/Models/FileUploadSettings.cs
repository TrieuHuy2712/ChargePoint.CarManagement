namespace ChargePoint.CarManagement.Models
{
    public class FileUploadSettings
    {
        public int MaxImageSizeMB { get; set; } = 10;
        public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

        public long MaxImageSizeBytes => MaxImageSizeMB * 1024L * 1024L;
    }
}
