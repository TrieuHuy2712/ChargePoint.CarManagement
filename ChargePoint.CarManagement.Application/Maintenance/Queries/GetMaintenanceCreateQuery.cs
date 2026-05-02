using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Domain.ViewModels.MaintenanceViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mediator;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ZiggyCreatures.Caching.Fusion;

namespace ChargePoint.CarManagement.Application.Maintenance.Queries
{
    public class GetMaintenanceCreateQuery : IRequest<MaintenanceCreateVM>
    {
        public int? CarId { get; set; }
    }

    public class GetMaintenanceCreateQueryHandler(
        IUnitOfWork unitOfWork,
        IAuthenService authenService,
        IFusionCache memoryCache) : IRequestHandler<GetMaintenanceCreateQuery, MaintenanceCreateVM>
    {
        private readonly IUnitOfWork unitOfWork = unitOfWork;
        private readonly IAuthenService authenService = authenService;
        private readonly IFusionCache memoryCache = memoryCache;

        public async ValueTask<MaintenanceCreateVM?> Handle(GetMaintenanceCreateQuery request, CancellationToken cancellationToken)
        {
            if(request.CarId == null)
            {
                // Nếu không có CarId, hiển thị dropdown để chọn xe
                var cars = await unitOfWork.Cars.GetAllAsync(cancellationToken);
                var selectListItems = new SelectList(cars, "Id", "BienSo"); // Now using Microsoft.AspNetCore.Mvc.Rendering.SelectList
                return new MaintenanceCreateVM
                {
                    SelectListCars = selectListItems,
                    Cars = cars,
                    MaintenanceRecord = new(),
                };
            }
            
            var car = await unitOfWork.Cars.GetByIdAsync(request.CarId.Value, cancellationToken);
            if (car == null) return null;

            MaintenanceRecord model;
            var draftKey = CacheKey.GetMaintenanceCreateDraftCacheKey(car.Id, authenService.GetCurrentUserName());
            var draftModel = memoryCache.TryGet<MaintenanceRecord>(draftKey);
            if (draftModel.HasValue && draftModel.Value != null)
            {
                draftModel.Value.CarId = car.Id;
                model = draftModel.Value;
            }
            else
            {
                model = new MaintenanceRecord
                {
                    CarId = car.Id,
                    NgayBaoDuong = DateTime.Now,
                    SoKmBaoDuong = car.OdoXe,
                    NguoiTao = authenService.GetCurrentUserName(),
                    CapBaoDuong = Domain.Enums.MaintenanceLevel.Cap1,
                    LoaiHoSo = Domain.Enums.DocumentType.BaoDuong,
                };
            }

            return new MaintenanceCreateVM
            {
                MaintenanceRecord = model,
                Cars = [car],
            };
        }
    }
}
