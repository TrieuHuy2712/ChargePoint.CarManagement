using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Common.Extensions;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Application.BulkTrafficViolation.Queries
{
    public class GetBulkTrafficViolationIndexQuery : IRequest<PagedResult<Domain.Entities.Car>>
    {
        public string Q { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class GetBulkTrafficViolationIndexQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBulkTrafficViolationIndexQuery, PagedResult<Domain.Entities.Car>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<PagedResult<Domain.Entities.Car>> Handle(GetBulkTrafficViolationIndexQuery query, CancellationToken cancellationToken)
        {
            var carQuery = await _unitOfWork.Cars.AsQueryable();

            // Apply search filter using reusable extension method
            carQuery = carQuery.ApplySearch(query.Q);

            var totalCount = await carQuery.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
            if (query.Page > totalPages && totalPages > 0) query.Page = totalPages;

            // Apply ordering and pagination using reusable extension methods
            var cars = await carQuery
                .ApplyDefaultOrdering()
                .ApplyPagination(query.Page, query.PageSize)
                .ToListAsync(cancellationToken);

            var viewModel = new PagedResult<Domain.Entities.Car>
            {
                Items = cars,
                PageNumber = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                SearchQuery = query.Q ?? string.Empty
            };

            return viewModel;
        }
    }
}
