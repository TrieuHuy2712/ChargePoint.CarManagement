using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.Maintenance.Commands
{
    public class DeleteMaintenanceCommand : IRequest<Result>
    {
        public MaintenanceRecord Model { get; set; }
    }

    public class DeleteMaintenanceHandler(
        IUnitOfWork unitOfWork,
        IImageUploadService imageUploadService,
        ILogger<DeleteMaintenanceHandler> logger) : IRequestHandler<DeleteMaintenanceCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly ILogger<DeleteMaintenanceHandler> _logger = logger;
        public async ValueTask<Result> Handle(DeleteMaintenanceCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var image in cmd.Model.DanhSachHinhAnh)
                {
                    await _imageUploadService.DeleteFileAsync(image);
                }
                _unitOfWork.MaintenanceRecords.Remove(cmd.Model);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bản ghi bảo dưỡng Id: {Id}", cmd.Model.Id);
                return Result.Fail("Đã xảy ra lỗi khi xóa bản ghi bảo dưỡng.");
            }
        }
    }
}
