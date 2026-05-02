using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Commands
{
    public class CreateTrafficViolationCommand : IRequest<Result>
    {
        public TrafficViolationCheck Model { get; set; }
    }

    public class CreateTrafficViolationHandler(
        IUnitOfWork unitOfWork,
        IAuthenService authenService,
        ILogger<CreateTrafficViolationHandler> logger) : IRequestHandler<CreateTrafficViolationCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IAuthenService _authenService = authenService;
        private readonly ILogger<CreateTrafficViolationHandler> _logger = logger;

        public async ValueTask<Result> Handle(CreateTrafficViolationCommand query, CancellationToken cancellationToken)
        {
            var newRecord = new TrafficViolationCheck
            {
                CarId = query.Model.CarId,
                NgayKiemTra = DateTime.Now,
                NguoiTao = _authenService.GetCurrentUserName(),
                CoViPham = query.Model.CoViPham,
                SoLuongViPham = query.Model.SoLuongViPham,
                NoiDungViPham = query.Model.NoiDungViPham,
                DiaDiemViPham = query.Model.DiaDiemViPham,
                NgayGioViPham = query.Model.NgayGioViPham,
                TrangThaiXuLy = query.Model.TrangThaiXuLy,
                GhiChu = query.Model.GhiChu
            };
            try
            {
                await _unitOfWork.TrafficViolations.AddAsync(newRecord, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating traffic violation record for CarId {CarId}", query.Model.CarId);
                return Result.Fail("An error occurred while creating the traffic violation record.");
            }
        }
    }
}
