using Microsoft.AspNetCore.Http;

namespace ChargePoint.CarManagement.Application.Interfaces
{
    public interface IImageUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string bienSo, string imageType);
        Task DeleteFileAsync(string fileUrl);
    }
}
