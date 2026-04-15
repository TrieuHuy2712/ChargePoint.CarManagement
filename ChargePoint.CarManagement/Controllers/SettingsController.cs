using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.ToListAsync();
            return View(settings);
        }

        // POST: Settings/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(IFormCollection form)
        {
            var dbSettings = await _context.SystemSettings.ToListAsync();

            foreach (var setting in dbSettings)
            {
                var inputName = $"settings[{setting.Key}]";

                if (form.ContainsKey(inputName))
                {
                    var values = form[inputName]; // Lấy danh sách các value trùng tên
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

            _context.UpdateRange(dbSettings);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Lưu thiết lập thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
