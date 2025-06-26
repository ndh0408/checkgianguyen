using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiaNguyenCheck.Services;
using GiaNguyenCheck.DTOs.CommonDTOs;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.Controllers
{
    [Authorize(Roles = "Staff")]
    [Route("Staff/[action]")]
    public class StaffController : Controller
    {
        private readonly ICheckInService _checkInService;
        private readonly IEventService _eventService;
        private readonly IGuestService _guestService;
        private readonly IDashboardService _dashboardService;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUserService _userService;

        public StaffController(
            ICheckInService checkInService,
            IEventService eventService,
            IGuestService guestService,
            IDashboardService dashboardService,
            ITenantProvider tenantProvider,
            IUserService userService)
        {
            _checkInService = checkInService;
            _eventService = eventService;
            _guestService = guestService;
            _dashboardService = dashboardService;
            _tenantProvider = tenantProvider;
            _userService = userService;
        }

        public async Task<IActionResult> CheckInScanner()
        {
            ViewData["Title"] = "Check-in Scanner";
            ViewData["Role"] = "Staff";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new CheckInScannerViewModel
            {
                TodaysEvents = await _eventService.GetTodaysEventsAsync(currentUser?.Id),
                RecentCheckins = await _checkInService.GetRecentCheckinsAsync(currentUser?.Id, 10),
                ScannerSettings = await _checkInService.GetScannerSettingsAsync(),
                QRCodeData = await _checkInService.GetQRCodeDataAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> TodaysEvents()
        {
            ViewData["Title"] = "Today's Events";
            ViewData["Role"] = "Staff";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new TodaysEventsViewModel
            {
                Events = await _eventService.GetTodaysEventsAsync(currentUser?.Id),
                EventDetails = await _eventService.GetEventDetailsAsync(currentUser?.Id),
                StaffAssignments = await _userService.GetStaffAssignmentsAsync(currentUser?.Id)
            };
            
            return View(model);
        }

        public async Task<IActionResult> SearchGuest()
        {
            ViewData["Title"] = "Search Guest";
            ViewData["Role"] = "Staff";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new SearchGuestViewModel
            {
                SearchHistory = await _guestService.GetSearchHistoryAsync(currentUser?.Id),
                GuestTemplates = await _guestService.GetGuestTemplatesAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> QuickStats()
        {
            ViewData["Title"] = "Quick Stats";
            ViewData["Role"] = "Staff";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new QuickStatsViewModel
            {
                TodayStats = await _checkInService.GetTodayStatsAsync(currentUser?.Id),
                WeeklyStats = await _checkInService.GetWeeklyStatsAsync(currentUser?.Id),
                PerformanceMetrics = await _userService.GetPerformanceMetricsAsync(currentUser?.Id),
                RecentActivity = await _checkInService.GetRecentActivityAsync(currentUser?.Id)
            };
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckIn(ProcessCheckInViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var result = await _checkInService.ProcessCheckInAsync(model, currentUser?.Id);
            
            return Json(new { 
                success = result.IsSuccess, 
                message = result.Message,
                guest = result.Guest,
                checkInTime = result.CheckInTime
            });
        }

        [HttpPost]
        public async Task<IActionResult> ManualCheckIn(ManualCheckInViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var result = await _checkInService.ManualCheckInAsync(model, currentUser?.Id);
            
            return Json(new { 
                success = result.IsSuccess, 
                message = result.Message,
                guest = result.Guest
            });
        }

        [HttpPost]
        public async Task<IActionResult> SearchGuestByName(SearchGuestByNameViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var guests = await _guestService.SearchGuestByNameAsync(model.Name, currentUser?.Id);
            
            return Json(new { 
                success = true, 
                guests = guests
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCheckInStatus(UpdateCheckInStatusViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var result = await _checkInService.UpdateCheckInStatusAsync(model, currentUser?.Id);
            
            return Json(new { 
                success = result.IsSuccess, 
                message = result.Message
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetEventGuests(string eventId)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var guests = await _guestService.GetEventGuestsAsync(eventId, currentUser?.Id);
            
            return Json(new { 
                success = true, 
                guests = guests
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCheckInStats(string eventId)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var stats = await _checkInService.GetCheckInStatsAsync(eventId, currentUser?.Id);
            
            return Json(new { 
                success = true, 
                stats = stats
            });
        }
    }

    public class CheckInScannerViewModel
    {
        public List<Event> TodaysEvents { get; set; } = new();
        public List<CheckIn> RecentCheckins { get; set; } = new();
        public ScannerSettings ScannerSettings { get; set; } = new();
        public QRCodeData QRCodeData { get; set; } = new();
    }

    public class TodaysEventsViewModel
    {
        public List<Event> Events { get; set; } = new();
        public List<EventDetail> EventDetails { get; set; } = new();
        public List<StaffAssignment> StaffAssignments { get; set; } = new();
    }

    public class SearchGuestViewModel
    {
        public List<SearchHistory> SearchHistory { get; set; } = new();
        public List<GuestTemplate> GuestTemplates { get; set; } = new();
    }

    public class QuickStatsViewModel
    {
        public TodayStats TodayStats { get; set; } = new();
        public WeeklyStats WeeklyStats { get; set; } = new();
        public PerformanceMetrics PerformanceMetrics { get; set; } = new();
        public List<RecentActivity> RecentActivity { get; set; } = new();
    }

    public class ProcessCheckInViewModel
    {
        public string QRCode { get; set; } = "";
        public string EventId { get; set; } = "";
        public string GuestId { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class ManualCheckInViewModel
    {
        public string GuestName { get; set; } = "";
        public string GuestEmail { get; set; } = "";
        public string GuestPhone { get; set; } = "";
        public string EventId { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class SearchGuestByNameViewModel
    {
        public string Name { get; set; } = "";
        public string EventId { get; set; } = "";
    }

    public class UpdateCheckInStatusViewModel
    {
        public string CheckInId { get; set; } = "";
        public string Status { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class ScannerSettings
    {
        public bool EnableSound { get; set; } = true;
        public bool EnableVibration { get; set; } = true;
        public string CameraResolution { get; set; } = "HD";
        public bool AutoFocus { get; set; } = true;
    }

    public class QRCodeData
    {
        public string EventId { get; set; } = "";
        public string EventName { get; set; } = "";
        public string GuestId { get; set; } = "";
        public string GuestName { get; set; } = "";
        public string QRCode { get; set; } = "";
    }

    public class EventDetail
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = "";
        public int TotalGuests { get; set; }
        public int CheckedInGuests { get; set; }
        public decimal CheckInRate { get; set; }
    }

    public class StaffAssignment
    {
        public string EventId { get; set; } = "";
        public string EventName { get; set; } = "";
        public string StaffId { get; set; } = "";
        public string StaffName { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime AssignmentDate { get; set; }
    }

    public class SearchHistory
    {
        public string Id { get; set; } = "";
        public string SearchTerm { get; set; } = "";
        public DateTime SearchDate { get; set; }
        public int ResultsCount { get; set; }
    }

    public class GuestTemplate
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class TodayStats
    {
        public int TotalCheckins { get; set; }
        public int SuccessfulCheckins { get; set; }
        public int FailedCheckins { get; set; }
        public decimal SuccessRate { get; set; }
        public int EventsWorked { get; set; }
    }

    public class WeeklyStats
    {
        public List<string> Days { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
        public List<decimal> SuccessRates { get; set; } = new();
    }

    public class PerformanceMetrics
    {
        public int TotalCheckins { get; set; }
        public decimal AverageCheckinTime { get; set; }
        public decimal AccuracyRate { get; set; }
        public int CustomerSatisfaction { get; set; }
    }

    public class RecentActivity
    {
        public string Id { get; set; } = "";
        public string Action { get; set; } = "";
        public string GuestName { get; set; } = "";
        public string EventName { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "";
    }

    public class CheckInResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public Guest? Guest { get; set; }
        public DateTime? CheckInTime { get; set; }
    }
} 