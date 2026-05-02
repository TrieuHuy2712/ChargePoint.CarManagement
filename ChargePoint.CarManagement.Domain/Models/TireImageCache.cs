using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace ChargePoint.CarManagement.Domain.Models
{
    public class TireImageCache
    {
        public List<CachedFileData> ChungTuFiles { get; set; } = new();
        public List<CachedFileData> DOTFiles { get; set; } = new();
    }

    public class CachedFileData
    {
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public byte[] Data { get; set; } = [];

        // ✅ Chuyển byte[] thành IFormFile để truyền vào UploadFileAsync
        public IFormFile ToFormFile()
        {
            var stream = new MemoryStream(Data);
            return new FormFile(stream, 0, Data.Length, "file", FileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = ContentType
            };
        }
    }
}
