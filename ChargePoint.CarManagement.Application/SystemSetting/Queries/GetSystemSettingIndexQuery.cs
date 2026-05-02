using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Domain.ViewModels;
using Mediator;

namespace ChargePoint.CarManagement.Application.SystemSetting.Queries
{
    public class GetSystemSettingIndexQuery : IRequest<SystemSettingIndexVM>
    {
    }

    public class GetSystemSettingIndexQueryHandler(
        IUnitOfWork unitOfWork,
        IAuthenService authenService) : IRequestHandler<GetSystemSettingIndexQuery, SystemSettingIndexVM>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IAuthenService _authenService = authenService;
        public async ValueTask<SystemSettingIndexVM> Handle(GetSystemSettingIndexQuery query, CancellationToken cancellationToken)
        {
            var settings = await _unitOfWork.SystemSettings.GetAllAsync(cancellationToken);
            bool isRootUser = _authenService.IsInRole(AppRoles.RootAdmin);
            var vm = new SystemSettingIndexVM
            {
                IsRootUser = isRootUser,
                Settings = settings.Select(s => new SystemSettingItemVM
                {
                    Key = s.Key,
                    Value = s.Value,
                    Description = s.Description,
                    Type = s.Type,
                    IsDisabled = (s.Key == SystemSettingKeys.MaintenanceMode && !isRootUser),
                    DisabledReason = (s.Key == SystemSettingKeys.MaintenanceMode && !isRootUser) ? "Chỉ root mới được thay đổi" : ""
                }).ToList()
            };
            return vm;
        }
    }
}
