using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace ChargePoint.CarManagement.Application.Interfaces.TireService
{
    public interface ITireImageService
    {
        /// <summary>
        /// Upload danh sách ảnh (cùng một nhóm files) cho nhiều vị trí lốp.
        /// Mỗi position sẽ nhận đủ tất cả files, kết quả trả về JSON-serialized url list.
        /// </summary>
        Task<Dictionary<ViTriLop, string>> UploadByPositionAsync(
            List<IFormFile>? files,
            IEnumerable<ViTriLop> positions,
            string bienSo,
            string prefix,
            DateTime ngayThucHien);

        /// <summary>
        /// Upload danh sách ảnh bảo dưỡng từ cached file data.
        /// </summary>
        Task<List<string>> UploadMaintenanceFilesAsync(
            List<CachedFileData> cachedFiles,
            string bienSo,
            string folderSuffix);
    }
}