using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.TireService;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ChargePoint.CarManagement.Infrastructure.Services
{
    public class TireImageService(
        IImageUploadService imageUploadService,
        ILogger<TireImageService> logger) : ITireImageService
    {
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly ILogger<TireImageService> _logger = logger;

        public async Task<Dictionary<ViTriLop, string>> UploadByPositionAsync(
            List<IFormFile>? files,
            IEnumerable<ViTriLop> positions,
            string bienSo,
            string prefix,
            DateTime ngayThucHien)
        {
            var result = new Dictionary<ViTriLop, string>();

            var validFiles = files?.Where(f => f?.Length > 0).ToList();
            if (validFiles == null || validFiles.Count == 0)
                return result;

            foreach (var position in positions)
            {
                var urls = new List<string>();
                foreach (var file in validFiles)
                {
                    try
                    {
                        var url = await _imageUploadService.UploadFileAsync(
                            file, bienSo, $"{prefix}_{position}_{ngayThucHien:yyyyMMdd}");
                        urls.Add(url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Lỗi upload ảnh {Prefix} vị trí {Position} xe {BienSo}",
                            prefix, position, bienSo);
                        throw;
                    }
                }
                result[position] = JsonSerializer.Serialize(urls);
            }

            return result;
        }

        public async Task<List<string>> UploadMaintenanceFilesAsync(
            List<CachedFileData> cachedFiles,
            string bienSo,
            string folderSuffix)
        {
            var urls = new List<string>();
            foreach (var cachedFile in cachedFiles)
            {
                var formFile = cachedFile.ToFormFile();
                var url = await _imageUploadService.UploadFileAsync(formFile, bienSo, folderSuffix);
                urls.Add(url);
            }
            return urls;
        }
    }
}