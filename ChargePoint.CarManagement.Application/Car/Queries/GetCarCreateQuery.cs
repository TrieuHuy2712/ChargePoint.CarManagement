using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using Mediator;

namespace ChargePoint.CarManagement.Application.Car.Queries
{
    public class GetCarCreateQuery : IRequest<Domain.Entities.Car>
    {
    }

    public class GetCarCreateQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCarCreateQuery, Domain.Entities.Car>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<Domain.Entities.Car> Handle(GetCarCreateQuery query, CancellationToken cancellationToken)
        {
            var nextId = await _unitOfWork.Cars.FindAsync(c => true, cancellationToken: cancellationToken)
                    .ContinueWith(t => t.Result.Count != 0 ? t.Result.Max(c => c.Stt) : 0, cancellationToken) + 1 ;
            return new Domain.Entities.Car
            {
               Stt = nextId,
            };
        }
    }
}
