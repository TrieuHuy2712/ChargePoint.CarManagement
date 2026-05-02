using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Application.Tire.Queries
{
    public class GetTireDetailByIdQuery : IRequest<TireRecord>
    {
        public int Id { get; set; }
    }

    public class GetTireDetailByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTireDetailByIdQuery, TireRecord>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<TireRecord?> Handle(GetTireDetailByIdQuery query, CancellationToken cancellationToken)
        {
            var tireRecord = await _unitOfWork.TireRecords.GetByIdAsync(query.Id,
                includeBuilder => includeBuilder.Include(tr => tr.Car)
                , cancellationToken);
            return tireRecord;
        }
    }
}
