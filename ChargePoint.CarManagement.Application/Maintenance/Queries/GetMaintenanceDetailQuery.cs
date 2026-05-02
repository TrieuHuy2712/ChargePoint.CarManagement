using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using Mediator;

namespace ChargePoint.CarManagement.Application.Maintenance.Queries
{
    public class GetMaintenanceDetailQuery : IRequest<MaintenanceRecord>
    {
        public int Id { get; set; }
    }

    public class GetMaintenanceDetailQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMaintenanceDetailQuery, MaintenanceRecord>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<MaintenanceRecord?> Handle(GetMaintenanceDetailQuery query, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.MaintenanceRecords.GetByIdAsync(query.Id, cancellationToken);

        }
    }
}
