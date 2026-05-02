using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ChargePoint.CarManagement.Application.SystemSetting.Commands
{
    public class SaveSystemSettingCommand : IRequest<Result>
    {
        public IFormCollection Form { get; set; }
    }

    public class SaveSystemSettingCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthenService authenService) : IRequestHandler<SaveSystemSettingCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IAuthenService _authenService = authenService;
        public async ValueTask<Result> Handle(SaveSystemSettingCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                var dbSettings = await _unitOfWork.SystemSettings.GetAllAsync(cancellationToken);
                bool isRoot = _authenService.IsInRole(AppRoles.RootAdmin);
                foreach (var setting in dbSettings)
                {
                    // Root validation constraint
                    if (setting.Key == SystemSettingKeys.MaintenanceMode && !isRoot)
                    {
                        continue; // Bỏ qua, không cho phép lưu thay đổi cấu hình này nếu không phải root
                    }

                    var inputName = $"settings[{setting.Key}]";

                    if (cmd.Form.ContainsKey(inputName))
                    {
                        var values = cmd.Form[inputName]; // Lấy danh sách các value trùng tên
                        if (setting.Type == "boolean")
                        {
                            // Nếu checkbox được tick, sẽ có "true" trong list values.
                            setting.Value = values.Contains("true") ? "true" : "false";
                        }
                        else
                        {
                            setting.Value = values.ToString();
                        }
                    }
                    else if (setting.Type == "boolean")
                    {
                        // Nếu unchecked, và cả hidden field không truyền lên thì mặc định là false
                        setting.Value = "false";
                    }
                }

                _unitOfWork.SystemSettings.UpdateRange(dbSettings);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging mechanism
                return Result.Fail("An error occurred while saving settings: " + ex.Message);
            }
        }
    }
}
