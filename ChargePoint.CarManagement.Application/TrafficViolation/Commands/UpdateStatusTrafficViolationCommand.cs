using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Commands
{
    public class UpdateStatusTrafficViolationCommand : IRequest<Result>
    {
        public TrafficViolationCheck Model { get; set; }
        public ViolationStatus TrangThaiXuLy { get; set; }
    }

    public class UpdateStatusTrafficViolationHandler(
        IUnitOfWork unitOfWork,
        IAuthenService authenService,
        ILogger<UpdateStatusTrafficViolationHandler> logger) : IRequestHandler<UpdateStatusTrafficViolationCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IAuthenService _authenService = authenService;
        private readonly ILogger<UpdateStatusTrafficViolationHandler> logger = logger;
        public async ValueTask<Result> Handle(UpdateStatusTrafficViolationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                command.Model.TrangThaiXuLy = command.TrangThaiXuLy;
                command.Model.NgayCapNhatTrangThai = DateTime.Now;
                command.Model.NguoiXuLy = _authenService.GetCurrentUserName();
                command.Model.NgayCapNhat = DateTime.Now;
                command.Model.NguoiCapNhat = authenService.GetCurrentUserName();

                _unitOfWork.TrafficViolations.Update(command.Model);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred while updating the traffic violation check with ID {Id}: {Message}", command.Model.Id, ex.Message);
                return Result.Fail($"An error occurred while updating the traffic violation check: {ex.Message}");
            }
        }
    }
}
