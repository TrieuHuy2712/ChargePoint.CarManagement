using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels.TireViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;


namespace ChargePoint.CarManagement.Application.Tire.Queries
{
    public class GetTireCarDetailQuery : IRequest<TireCareDetailVM>
    {
        public int? CarId { get; set; }
    }


    public class GetTireCarDetailQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTireCarDetailQuery, TireCareDetailVM>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<TireCareDetailVM?> Handle(GetTireCarDetailQuery query, CancellationToken cancellationToken)
        {
            if (query.CarId is null) return null;

            // Load car
            var car = await _unitOfWork.Cars.GetByIdAsync(query.CarId.Value, cancellationToken);
            if (car is null) return null;

            // Optimize for MySQL: Get latest tire record for each position efficiently
            // This leverages MySQL indexes on (CarId, ViTriLop, NgayThucHien)
            var tireRecordsQuery = await _unitOfWork.TireRecords.AsQueryable();

            var latestRecordsPerPosition = await tireRecordsQuery
                .Where(t => t.CarId == query.CarId.Value)
                .GroupBy(t => t.ViTriLop)
                .Select(g => new TireRecordDetailIndexVM
                {
                    ViTri = g.Key,
                    LastRecord = g.OrderByDescending(t => t.NgayThucHien)
                                  .ThenByDescending(t => t.Id)
                                  .FirstOrDefault()
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new TireCareDetailVM 
            { 
                Car = car, 
                TireRecordsPosition = latestRecordsPerPosition 
            };
        }
    }
}
