using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using Mediator;

namespace ChargePoint.CarManagement.Application.Car.Queries
{
    public class GetCarVINExistQuery : IRequest<bool>
    {
        public string VINNumber { get; set; }
    }

    public class GetCarVINExistQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCarVINExistQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<bool> Handle(GetCarVINExistQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Cars.AnyAsync(c => c.SoVIN == query.VINNumber, cancellationToken);
        }
    }
}
