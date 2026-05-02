using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels;
using Mediator;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Queries
{
    public class GetTrafficViolationHistoryQuery : IRequest<TrafficViolationHistoryVM>
    {
        public int Id { get; set; }
        public Domain.Entities.Car Car { get; set; }
    }
    public class GetTrafficViolationHistoryQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTrafficViolationHistoryQuery, TrafficViolationHistoryVM>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<TrafficViolationHistoryVM> Handle(GetTrafficViolationHistoryQuery query, CancellationToken cancellationToken)
        {
            var checks = await _unitOfWork.TrafficViolations
                .FindAsync(
                    t => t.CarId == query.Id,
                    includeBuilder: q => q.OrderByDescending(t => t.NgayKiemTra),
                    cancellationToken: cancellationToken
                );

            return new TrafficViolationHistoryVM
            {
                Car = query.Car,
                Checks = checks
            };
        }
    }
}
