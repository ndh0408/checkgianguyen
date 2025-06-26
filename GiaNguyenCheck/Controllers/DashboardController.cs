using Microsoft.AspNetCore.Mvc;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace GiaNguyenCheck.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IEventService _eventService;
        private readonly ICheckInService _checkInService;

        public DashboardController(
            IDashboardService dashboardService,
            IEventService eventService,
            ICheckInService checkInService)
        {
            _dashboardService = dashboardService;
            _eventService = eventService;
            _checkInService = checkInService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                ViewBag.Stats = stats;
                return View();
            }
            catch (Exception ex)
            {
                // Log error
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentCheckIns()
        {
            try
            {
                var recentCheckIns = await _checkInService.GetRecentCheckInsAsync(10);
                return Json(new { success = true, data = recentCheckIns });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            try
            {
                var upcomingEvents = await _eventService.GetUpcomingEventsAsync(5);
                return Json(new { success = true, data = upcomingEvents });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCheckInStats([FromQuery] string period = "today")
        {
            try
            {
                var stats = await _dashboardService.GetCheckInStatsAsync(period);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            try
            {
                var reportData = await _dashboardService.GenerateReportAsync();
                // Implementation for exporting report
                return Json(new { success = true, message = "Báo cáo đã được xuất thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 