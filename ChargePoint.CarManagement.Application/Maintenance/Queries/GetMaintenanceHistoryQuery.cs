using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels.MaintenanceViewModels;
using Mediator;

namespace ChargePoint.CarManagement.Application.Maintenance.Queries
{
    public class GetMaintenanceHistoryQuery : IRequest<MaintenanceHistoryViewModel>
    {
        public required Domain.Entities.Car Car { get; set; }
        public int CarId { get; set; }
    }

    public class GetMaintenanceHistoryQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMaintenanceHistoryQuery, MaintenanceHistoryViewModel>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<MaintenanceHistoryViewModel> Handle(GetMaintenanceHistoryQuery query, CancellationToken cancellationToken = default)
        {
            var maintenanceRecords = await _unitOfWork.MaintenanceRecords.FindAsync(t => t.CarId == query.CarId, cancellationToken: cancellationToken);
            return new MaintenanceHistoryViewModel
            {
                Car = query.Car,
                MaintenanceRecords = maintenanceRecords
            };
        }
    }
}
