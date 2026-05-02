using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Infrastructure.Persistence.Data;
using ChargePoint.CarManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Middleware
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Bỏ qua các file tĩnh
            if (context.Request.Path.Value != null && context.Request.Path.Value.StartsWith("/lib") || context.Request.Path.Value?.StartsWith("/css") == true || context.Request.Path.Value?.StartsWith("/js") == true)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Bỏ qua path đăng nhập, logout, access denied, và trang báo trì
            if (path == "/home/maintenance" || path.StartsWith("/account/login") || path.StartsWith("/account/logout"))
            {
                await _next(context);
                return;
            }

            // Kiểm tra trạng thái bảo trì trong DB
            bool isMaintenanceMode = false;

            // Resolve DbContext từ RequestServices vì DbContext là Scoped
            var dbContext = context.RequestServices.GetService<ApplicationDbContext>();
            if (dbContext != null)
            {
                var setting = await dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.MaintenanceMode);
                isMaintenanceMode = setting?.Value == "true";
            }

            if (isMaintenanceMode)
            {
                // Kiểm tra xem User hiện tại có phải là RootAdmin hay không
                bool isRoot = context.User.Identity?.IsAuthenticated == true && context.User.IsInRole(AppRoles.RootAdmin);

                if (!isRoot)
                {
                    context.Response.Redirect("/Home/Maintenance");
                    return;
                }
            }

            await _next(context);
        }
    }
}
