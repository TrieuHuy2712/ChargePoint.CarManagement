using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Application.Interfaces.TireService;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ChargePoint.CarManagement.Application.Tire.Commands
{
    public class DeleteTireImageCommand : IRequest<Result>
    {
        public int RecordId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageType { get; set; } = "ChungTu"; // "ChungTu" or "DOT"
    }

    public class DeleteTireImageHandler(
        IUnitOfWork unitOfWork, 
        IImageUploadService imageUploadService, 
        ILogger<DeleteTireImageHandler> logger) : IRequestHandler<DeleteTireImageCommand, Result>
    {
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DeleteTireImageHandler> _logger = logger;

        public async ValueTask<Result> Handle(DeleteTireImageCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                var record = await _unitOfWork.TireRecords.GetByIdAsync(cmd.RecordId, cancellationToken);
                if (record == null)
                {
                    return Result.Fail("Không tìm thấy hồ sơ");
                }

                bool imageFound = false;
                // Xóa ảnh chứng từ hoặc ảnh DOT tùy theo imageType
                if (cmd.ImageType == "DOT")
                {
                    var dotImages = record.DanhSachHinhAnhDOT;
                    if (dotImages.Contains(cmd.ImageUrl))
                    {
                        await _imageUploadService.DeleteFileAsync(cmd.ImageUrl);
                        dotImages.Remove(cmd.ImageUrl);
                        record.HinhAnhDOT = JsonSerializer.Serialize(dotImages);
                        imageFound = true;
                    }
                }
                else
                {
                    var images = record.DanhSachHinhAnh;
                    if (images.Contains(cmd.ImageUrl))
                    {
                        await _imageUploadService.DeleteFileAsync(cmd.ImageUrl);
                        images.Remove(cmd.ImageUrl);
                        record.HinhAnhChungTu = JsonSerializer.Serialize(images);
                        imageFound = true;
                    }
                }

                if (imageFound)
                {
                    _unitOfWork.TireRecords.Update(record);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return Result.Ok();
                }
                return Result.Fail("Không tìm thấy hình ảnh");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tire image for record {RecordId}", cmd.RecordId);
                return Result.Fail($"Error deleting tire image: {ex.Message}");
            }
        }
    }
}
