using ChargePoint.CarManagement.Application.Common.Extensions;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Application.Car.Queries
{
    public class GetCarIndexQuery : IRequest<PagedResult<Domain.Entities.Car>>
    {
        public string Q { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetCarIndexQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCarIndexQuery, PagedResult<Domain.Entities.Car>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<PagedResult<Domain.Entities.Car>> Handle(GetCarIndexQuery query, CancellationToken cancellationToken)
        {
            var carsQuery = await _unitOfWork.Cars.AsQueryable();

            // Apply search filter using reusable extension method
            carsQuery = carsQuery.ApplySearch(query.Q);

            var totalCount = await carsQuery.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
            if (query.PageNumber > totalPages && totalPages > 0) query.PageNumber = totalPages;

            // Apply ordering and pagination using reusable extension methods
            var cars = await carsQuery
                .ApplyDefaultOrdering()
                .ApplyPagination(query.PageNumber, query.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);

            return new PagedResult<Domain.Entities.Car>
            {
                Items = cars,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                SearchQuery = query.Q
            };
        }
    }
}
