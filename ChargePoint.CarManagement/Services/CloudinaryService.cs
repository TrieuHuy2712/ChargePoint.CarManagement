using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ChargePoint.CarManagement.Models;
using Microsoft.Extensions.Options;

namespace ChargePoint.CarManagement.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string bienSo, string imageType);
        Task<string> UploadVideoAsync(IFormFile file, string bienSo, string videoType);
        Task DeleteFileAsync(string fileUrl);
        Task DeleteVideoAsync(string fileUrl);
    }

    public class CloudinaryService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _cloudinarySettings;
        private readonly FileUploadSettings _fileUploadSettings;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(
            IOptions<CloudinarySettings> cloudinarySettings,
            IOptions<FileUploadSettings> fileUploadSettings,
            ILogger<CloudinaryService> logger)
        {
            _cloudinarySettings = cloudinarySettings.Value;
            _fileUploadSettings = fileUploadSettings.Value;
            _logger = logger;

            var account = new Account(
                _cloudinarySettings.CloudName,
                _cloudinarySettings.ApiKey,
                _cloudinarySettings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string bienSo, string imageType)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // Validate image
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_fileUploadSettings.AllowedImageExtensions.Contains(extension))
            {
                throw new ArgumentException($"Định dạng hình ảnh không hợp lệ. Chỉ chấp nhận: {string.Join(", ", _fileUploadSettings.AllowedImageExtensions)}");
            }

            if (file.Length > _fileUploadSettings.MaxImageSizeBytes)
            {
                throw new ArgumentException($"Hình ảnh quá lớn. Tối đa {_fileUploadSettings.MaxImageSizeMB}MB");
            }

            var folderName = string.IsNullOrEmpty(bienSo) ? "NoPlate" : bienSo.Replace(" ", "_");
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var publicId = $"{folderName}_{imageType}_{timestamp}";

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId,
                Folder = $"{_cloudinarySettings.RootFolder}/{folderName}",
                Overwrite = true,
                Invalidate = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary image upload failed: {Error}", uploadResult.Error.Message);
                throw new Exception($"Upload failed: {uploadResult.Error.Message}");
            }

            _logger.LogInformation("Uploaded image to Cloudinary: {PublicId}", uploadResult.PublicId);
            return uploadResult.SecureUrl.ToString();
        }

        public async Task<string> UploadVideoAsync(IFormFile file, string bienSo, string videoType)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // Validate video
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_fileUploadSettings.AllowedVideoExtensions.Contains(extension))
            {
                throw new ArgumentException($"Định dạng video không hợp lệ. Chỉ chấp nhận: {string.Join(", ", _fileUploadSettings.AllowedVideoExtensions)}");
            }

            if (file.Length > _fileUploadSettings.MaxVideoSizeBytes)
            {
                throw new ArgumentException($"Video quá lớn. Tối đa {_fileUploadSettings.MaxVideoSizeMB}MB");
            }

            var folderName = string.IsNullOrEmpty(bienSo) ? "NoPlate" : bienSo.Replace(" ", "_");
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var publicId = $"{folderName}_{videoType}_{timestamp}";

            await using var stream = file.OpenReadStream();

            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId,
                Folder = $"{_cloudinarySettings.RootFolder}/{folderName}/videos",
                Overwrite = true,
                Invalidate = true,
                EagerTransforms =
                [
                    new Transformation().Width(320).Height(240).Crop("fill").FetchFormat("jpg")
                ],
                EagerAsync = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary video upload failed: {Error}", uploadResult.Error.Message);
                throw new Exception($"Upload video failed: {uploadResult.Error.Message}");
            }

            _logger.LogInformation("Uploaded video to Cloudinary: {PublicId}, Duration: {Duration}s",
                uploadResult.PublicId, uploadResult.Duration);

            return uploadResult.SecureUrl.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            await DeleteResourceAsync(fileUrl, ResourceType.Image);
        }

        public async Task DeleteVideoAsync(string fileUrl)
        {
            await DeleteResourceAsync(fileUrl, ResourceType.Video);
        }

        private async Task DeleteResourceAsync(string fileUrl, ResourceType resourceType)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var publicId = ExtractPublicIdFromUrl(fileUrl);
            if (string.IsNullOrEmpty(publicId)) return;

            try
            {
                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = resourceType,
                    Invalidate = true
                };

                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Result == "ok")
                {
                    _logger.LogInformation("Deleted {Type}: {PublicId}", resourceType, publicId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete {Type}: {PublicId}, Result: {Result}",
                        resourceType, publicId, result.Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting {Type}: {PublicId}", resourceType, publicId);
            }
        }

        private string ExtractPublicIdFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;

                var uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex == -1) return string.Empty;

                var afterUpload = path[(uploadIndex + 8)..];

                if (afterUpload.StartsWith('v') && afterUpload.Contains('/'))
                {
                    var versionEnd = afterUpload.IndexOf('/');
                    afterUpload = afterUpload[(versionEnd + 1)..];
                }

                var lastDot = afterUpload.LastIndexOf('.');
                if (lastDot > 0)
                {
                    afterUpload = afterUpload[..lastDot];
                }

                return afterUpload;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract public_id from URL: {Url}", url);
                return string.Empty;
            }
        }
    }
}
