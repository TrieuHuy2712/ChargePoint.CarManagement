using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.Tire.Commands
{
    public class DeleteTireCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
    }

    public class DeleteTireHandler(
        IUnitOfWork unitOfWork,
        IImageUploadService imageUploadService,
        ILogger<DeleteTireHandler> logger) : IRequestHandler<DeleteTireCommand, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly ILogger<DeleteTireHandler> _logger = logger;
        public async ValueTask<Result<int>> Handle(DeleteTireCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                var record = await _unitOfWork.TireRecords.GetByIdAsync(cmd.Id, cancellationToken);
                if (record == null)
                {
                    return Result<int>.Fail("Không tìm thấy hồ sơ");
                }
                // Xóa tất cả ảnh liên quan đến hồ sơ lốp
                var allImages = record.DanhSachHinhAnh.Concat(record.DanhSachHinhAnhDOT).ToList();
                foreach (var imageUrl in allImages)
                {
                    await _imageUploadService.DeleteFileAsync(imageUrl);
                }
                // Xóa hồ sơ lốp
                _unitOfWork.TireRecords.Remove(record);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<int>.Ok(record.CarId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hồ sơ lốp với ID {Id}", cmd.Id);
                return Result<int>.Fail("Đã xảy ra lỗi khi xóa hồ sơ lốp");
            }
        }
    }
}
