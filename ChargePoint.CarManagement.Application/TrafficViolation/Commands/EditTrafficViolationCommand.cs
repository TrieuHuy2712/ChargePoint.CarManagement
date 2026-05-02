using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Commands
{
    public class EditTrafficViolationCommand : IRequest<Result>
    {
        public TrafficViolationCheck Model { get; set; }
    }

    public class EditTrafficViolationHandler(
        IUnitOfWork unitOfWork,
        IAuthenService authenService,
        ILogger<EditTrafficViolationHandler> logger) : IRequestHandler<EditTrafficViolationCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IAuthenService _authenService = authenService;
        private readonly ILogger<EditTrafficViolationHandler> logger = logger;
        public async ValueTask<Result> Handle(EditTrafficViolationCommand command, CancellationToken cancellationToken)
        {
            var existingRecord = await _unitOfWork.TrafficViolations.GetByIdAsync(command.Model.Id, cancellationToken);
            if (existingRecord == null)
            {
                return Result.Fail($"Traffic violation check with ID {command.Model.Id} not found.");
            }
            try
            {
                var model = command.Model;
                existingRecord.SoLuongViPham = model.SoLuongViPham;
                existingRecord.CoViPham = model.SoLuongViPham > 0;
                existingRecord.NgayGioViPham = model.NgayGioViPham;
                existingRecord.NoiDungViPham = model.NoiDungViPham;
                existingRecord.DiaDiemViPham = model.DiaDiemViPham;
                existingRecord.TrangThaiXuLy = model.TrangThaiXuLy;
                existingRecord.NgayCapNhatTrangThai = model.NgayCapNhatTrangThai;
                existingRecord.NguoiXuLy = model.NguoiXuLy;
                existingRecord.GhiChu = model.GhiChu;
                existingRecord.NgayCapNhat = DateTime.Now;
                existingRecord.NguoiCapNhat = authenService.GetCurrentUserName();

                _unitOfWork.TrafficViolations.Update(existingRecord);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!await _unitOfWork.TrafficViolations.AnyAsync(t => t.Id == command.Model.Id, cancellationToken))
                {
                    return Result.Fail($"Traffic violation check with ID {command.Model.Id} no longer exists.");
                }
                else
                {
                    logger.LogError("A concurrency error occurred while updating the traffic violation check with ID {Id}", command.Model.Id);
                    return Result.Fail("A concurrency error occurred while updating the traffic violation check. Please try again.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating the traffic violation check with ID {Id}", command.Model.Id);
                return Result.Fail($"An error occurred while updating the traffic violation check: {ex.Message}");
            }
            
        }
    }
}
