using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.Maintenance.Commands
{
    public class DeleteImageMaintenaceCommand : IRequest<Result>
    {
        public List<string> ImageUrls { get; set; }

        public string ImageUrl { get; set; }

        public MaintenanceRecord Model { get; set; }
    }

    public class DeleteImageMaintenaceHandler(
        IImageUploadService imageUploadService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteImageMaintenaceHandler> logger) : IRequestHandler<DeleteImageMaintenaceCommand, Result>
    {
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DeleteImageMaintenaceHandler> _logger = logger;
        public async ValueTask<Result> Handle(DeleteImageMaintenaceCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                if (cmd.ImageUrls.Contains(cmd.ImageUrl))
                {
                    await _imageUploadService.DeleteFileAsync(cmd.ImageUrl);
                    var record = await _unitOfWork.MaintenanceRecords.FirstOrDefaultAsync(m => m.DanhSachHinhAnh.Contains(cmd.ImageUrl)
                        && m.Id == cmd.Model.Id, cancellationToken);
                    if (record != null)
                    {
                        record.DanhSachHinhAnh.Remove(cmd.ImageUrl);
                        _unitOfWork.MaintenanceRecords.Update(record);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        return Result.Ok();
                    }
                    else {                         
                        _logger.LogWarning("Không tìm thấy bản ghi bảo dưỡng chứa hình ảnh: {ImageUrl}", cmd.ImageUrl);
                        return Result.Fail("Không tìm thấy bản ghi bảo dưỡng chứa hình ảnh.");
                    }
                }
                return Result.Fail("Hình ảnh không tồn tại trong danh sách hình ảnh bảo dưỡng.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hình ảnh bảo dưỡng: {ImageUrls}", string.Join(", ", cmd.ImageUrls));
                return Result.Fail("Đã xảy ra lỗi khi xóa hình ảnh bảo dưỡng.");
            }
        }
    }
}
