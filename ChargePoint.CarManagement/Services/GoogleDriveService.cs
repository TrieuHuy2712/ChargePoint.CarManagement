using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using ChargePoint.CarManagement.Models;
using Microsoft.Extensions.Options;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;

namespace ChargePoint.CarManagement.Services
{
    public interface IGoogleDriveService
    {
        Task<string> UploadFileAsync(IFormFile file, string bienSo, string imageType);
        Task DeleteFileAsync(string fileUrl);
    }

    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly GoogleDriveSettings _settings;
        private readonly ILogger<GoogleDriveService> _logger;

        public GoogleDriveService(IOptions<GoogleDriveSettings> settings, ILogger<GoogleDriveService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var credential = new UserCredential(
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _settings.ClientId,
                        ClientSecret = _settings.ClientSecret
                    },
                    Scopes = new[] { DriveService.Scope.Drive }
                }),
                "user",
                new TokenResponse { RefreshToken = _settings.RefreshToken }
            );

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _settings.ApplicationName
            });
        }

        public async Task<string> UploadFileAsync(IFormFile file, string bienSo, string imageType)
        {
            var folderName = string.IsNullOrEmpty(bienSo) ? "NoPlate" : bienSo.Replace(" ", "_");
            var folderId = await GetOrCreateFolderAsync(folderName);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{folderName}_{imageType}_{timestamp}{extension}";

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = new List<string> { folderId }
            };

            using var stream = file.OpenReadStream();
            var uploadRequest = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
            uploadRequest.Fields = "id";
            uploadRequest.SupportsAllDrives = true;

            var uploadResult = await uploadRequest.UploadAsync();

            if (uploadResult.Status != UploadStatus.Completed)
                throw new Exception($"Upload failed: {uploadResult.Exception?.Message}");

            return $"https://drive.google.com/uc?export=view&id={uploadRequest.ResponseBody.Id}";
        }

        private async Task<string> GetOrCreateFolderAsync(string folderName)
        {
            // Tìm folder trong RootFolderId (đây là folder thường, không phải Shared Drive)
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"name='{folderName}' and mimeType='application/vnd.google-apps.folder' and '{_settings.RootFolderId}' in parents and trashed=false";
            listRequest.Fields = "files(id, name)";
            listRequest.SupportsAllDrives = true;
            listRequest.IncludeItemsFromAllDrives = true;
            // Bỏ DriveId và Corpora vì RootFolderId là folder thường

            var result = await listRequest.ExecuteAsync();

            if (result.Files?.Count > 0)
                return result.Files[0].Id;

            // Tạo folder mới trong RootFolderId
            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { _settings.RootFolderId }
            };

            var createRequest = _driveService.Files.Create(folderMetadata);
            createRequest.Fields = "id";
            createRequest.SupportsAllDrives = true;

            var folder = await createRequest.ExecuteAsync();
            _logger.LogInformation("Created folder {FolderName} with ID {FolderId}", folderName, folder.Id);
            return folder.Id;
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var fileId = ExtractFileIdFromUrl(fileUrl);
            if (!string.IsNullOrEmpty(fileId))
            {
                try
                {
                    var deleteRequest = _driveService.Files.Delete(fileId);
                    deleteRequest.SupportsAllDrives = true;
                    await deleteRequest.ExecuteAsync();
                    _logger.LogInformation("Deleted file {FileId}", fileId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file {FileId}", fileId);
                }
            }
        }

        private string ExtractFileIdFromUrl(string url)
        {
            if (url.Contains("id="))
            {
                var startIndex = url.IndexOf("id=") + 3;
                var endIndex = url.IndexOf("&", startIndex);
                if (endIndex == -1) endIndex = url.Length;
                return url.Substring(startIndex, endIndex - startIndex);
            }
            return string.Empty;
        }
    }
}
