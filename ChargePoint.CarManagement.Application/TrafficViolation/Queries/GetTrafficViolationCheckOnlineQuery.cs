using ChargePoint.CarManagement.Domain.Models.TrafficViolation;
using Mediator;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Queries
{
    public class GetTrafficViolationCheckOnlineQuery : IRequest<TrafficViolationResult>
    {
        public string BienSo { get; set; }
    }

    public class GetTrafficViolationCheckOnlineQueryHandler(Interfaces.ITrafficViolationService trafficViolationService) : IRequestHandler<GetTrafficViolationCheckOnlineQuery, TrafficViolationResult>
    {
        private readonly Interfaces.ITrafficViolationService trafficViolationService = trafficViolationService;
        public async ValueTask<TrafficViolationResult> Handle(GetTrafficViolationCheckOnlineQuery query, CancellationToken cancellationToken = default)
        {
            var result = await trafficViolationService.CheckViolationAsync(query.BienSo);
            return result;
        }
    }
}
