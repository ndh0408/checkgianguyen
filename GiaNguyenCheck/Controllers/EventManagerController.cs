using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiaNguyenCheck.Services;
using GiaNguyenCheck.DTOs.CommonDTOs;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.Controllers
{
    [Authorize(Roles = "EventManager")]
    [Route("EventManager/[action]")]
    public class EventManagerController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IGuestService _guestService;
        private readonly ICheckInService _checkInService;
        private readonly IEmailService _emailService;
        private readonly IDashboardService _dashboardService;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUserService _userService;

        public EventManagerController(
            IEventService eventService,
            IGuestService guestService,
            ICheckInService checkInService,
            IEmailService emailService,
            IDashboardService dashboardService,
            ITenantProvider tenantProvider,
            IUserService userService)
        {
            _eventService = eventService;
            _guestService = guestService;
            _checkInService = checkInService;
            _emailService = emailService;
            _dashboardService = dashboardService;
            _tenantProvider = tenantProvider;
            _userService = userService;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "My Dashboard";
            ViewData["Role"] = "EventManager";
            
            var currentTenant = _tenantProvider.GetCurrentTenant();
            var currentUser = await _userService.GetCurrentUserAsync();
            
            var model = new EventManagerDashboardViewModel
            {
                MyEvents = await _eventService.GetEventsByManagerAsync(currentUser?.Id),
                UpcomingEvents = await _eventService.GetUpcomingEventsAsync(currentUser?.Id, 5),
                RecentCheckins = await _checkInService.GetRecentCheckinsAsync(currentUser?.Id, 10),
                EventStatistics = await _eventService.GetEventStatisticsAsync(currentUser?.Id),
                GuestStatistics = await _guestService.GetGuestStatisticsAsync(currentUser?.Id),
                QuickActions = await _dashboardService.GetQuickActionsAsync(currentUser?.Id)
            };
            
            return View(model);
        }

        public async Task<IActionResult> EventManagement()
        {
            ViewData["Title"] = "Event Management";
            ViewData["Role"] = "EventManager";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new EventManagementViewModel
            {
                MyEvents = await _eventService.GetEventsByManagerAsync(currentUser?.Id),
                EventTemplates = await _eventService.GetEventTemplatesAsync(),
                AvailableStaff = await _userService.GetAvailableStaffAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> GuestManagement()
        {
            ViewData["Title"] = "Guest Management";
            ViewData["Role"] = "EventManager";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new GuestManagementViewModel
            {
                GuestLists = await _guestService.GetGuestListsAsync(currentUser?.Id),
                ImportHistory = await _guestService.GetImportHistoryAsync(currentUser?.Id),
                InvitationStatus = await _emailService.GetInvitationStatusAsync(currentUser?.Id)
            };
            
            return View(model);
        }

        public async Task<IActionResult> CheckInOverview()
        {
            ViewData["Title"] = "Check-in Overview";
            ViewData["Role"] = "EventManager";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new CheckInOverviewViewModel
            {
                LiveStatistics = await _checkInService.GetLiveStatisticsAsync(currentUser?.Id),
                CheckInHistory = await _checkInService.GetCheckInHistoryAsync(currentUser?.Id),
                AssignedStaff = await _userService.GetAssignedStaffAsync(currentUser?.Id),
                QRCodeSettings = await _checkInService.GetQRCodeSettingsAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> Communications()
        {
            ViewData["Title"] = "Communications";
            ViewData["Role"] = "EventManager";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new CommunicationsViewModel
            {
                EmailCampaigns = await _emailService.GetEmailCampaignsAsync(currentUser?.Id),
                SMSTemplates = await _emailService.GetSMSTemplatesAsync(),
                SentEmails = await _emailService.GetSentEmailsAsync(currentUser?.Id),
                EmailTemplates = await _emailService.GetEmailTemplatesAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> Reports()
        {
            ViewData["Title"] = "Reports";
            ViewData["Role"] = "EventManager";
            
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new ReportsViewModel
            {
                EventAnalytics = await _eventService.GetEventAnalyticsAsync(currentUser?.Id),
                GuestAnalytics = await _guestService.GetGuestAnalyticsAsync(currentUser?.Id),
                CheckInReports = await _checkInService.GetCheckInReportsAsync(currentUser?.Id),
                ExportOptions = await _dashboardService.GetExportOptionsAsync()
            };
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            await _eventService.CreateEventAsync(model, currentUser?.Id);
            return RedirectToAction(nameof(EventManagement));
        }

        [HttpPost]
        public async Task<IActionResult> ImportGuests(IFormFile file, string eventId)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            await _guestService.ImportGuestsAsync(file, eventId, currentUser?.Id);
            return RedirectToAction(nameof(GuestManagement));
        }

        [HttpPost]
        public async Task<IActionResult> SendInvitations(SendInvitationViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            await _emailService.SendInvitationsAsync(model, currentUser?.Id);
            return RedirectToAction(nameof(Communications));
        }

        [HttpPost]
        public async Task<IActionResult> AssignStaff(AssignStaffViewModel model)
        {
            await _eventService.AssignStaffAsync(model);
            return RedirectToAction(nameof(EventManagement));
        }

        [HttpPost]
        public async Task<IActionResult> ExportReport(ExportReportViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var reportData = await _dashboardService.GenerateReportAsync(model, currentUser?.Id);
            return File(reportData, "application/pdf", $"report_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }

    public class EventManagerDashboardViewModel
    {
        public List<Event> MyEvents { get; set; } = new();
        public List<Event> UpcomingEvents { get; set; } = new();
        public List<CheckIn> RecentCheckins { get; set; } = new();
        public EventStatistics EventStatistics { get; set; } = new();
        public GuestStatistics GuestStatistics { get; set; } = new();
        public List<QuickAction> QuickActions { get; set; } = new();
    }

    public class EventManagementViewModel
    {
        public List<Event> MyEvents { get; set; } = new();
        public List<EventTemplate> EventTemplates { get; set; } = new();
        public List<User> AvailableStaff { get; set; } = new();
    }

    public class GuestManagementViewModel
    {
        public List<GuestList> GuestLists { get; set; } = new();
        public List<ImportHistory> ImportHistory { get; set; } = new();
        public InvitationStatus InvitationStatus { get; set; } = new();
    }

    public class CheckInOverviewViewModel
    {
        public LiveStatistics LiveStatistics { get; set; } = new();
        public List<CheckIn> CheckInHistory { get; set; } = new();
        public List<User> AssignedStaff { get; set; } = new();
        public QRCodeSettings QRCodeSettings { get; set; } = new();
    }

    public class CommunicationsViewModel
    {
        public List<EmailCampaign> EmailCampaigns { get; set; } = new();
        public List<SMSTemplate> SMSTemplates { get; set; } = new();
        public List<SentEmail> SentEmails { get; set; } = new();
        public List<EmailTemplate> EmailTemplates { get; set; } = new();
    }

    public class ReportsViewModel
    {
        public EventAnalytics EventAnalytics { get; set; } = new();
        public GuestAnalytics GuestAnalytics { get; set; } = new();
        public CheckInReports CheckInReports { get; set; } = new();
        public List<ExportOption> ExportOptions { get; set; } = new();
    }

    public class CreateEventViewModel
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = "";
        public int MaxGuests { get; set; }
        public string EventType { get; set; } = "";
        public List<string> AssignedStaffIds { get; set; } = new();
    }

    public class SendInvitationViewModel
    {
        public string EventId { get; set; } = "";
        public string TemplateId { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
        public List<string> GuestIds { get; set; } = new();
    }

    public class AssignStaffViewModel
    {
        public string EventId { get; set; } = "";
        public List<string> StaffIds { get; set; } = new();
        public string Role { get; set; } = "";
    }

    public class ExportReportViewModel
    {
        public string ReportType { get; set; } = "";
        public string EventId { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } = "PDF";
    }

    public class EventStatistics
    {
        public int TotalEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int CompletedEvents { get; set; }
        public decimal AverageAttendance { get; set; }
    }

    public class GuestStatistics
    {
        public int TotalGuests { get; set; }
        public int InvitedGuests { get; set; }
        public int AttendedGuests { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    public class QuickAction
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Url { get; set; } = "";
        public string Color { get; set; } = "";
    }

    public class EventTemplate
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class GuestList
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int GuestCount { get; set; }
        public string EventName { get; set; } = "";
    }

    public class ImportHistory
    {
        public string Id { get; set; } = "";
        public string FileName { get; set; } = "";
        public DateTime ImportDate { get; set; }
        public int ImportedCount { get; set; }
        public string Status { get; set; } = "";
    }

    public class InvitationStatus
    {
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
    }

    public class LiveStatistics
    {
        public int TotalCheckins { get; set; }
        public int TodayCheckins { get; set; }
        public int PendingCheckins { get; set; }
        public decimal CheckinRate { get; set; }
    }

    public class QRCodeSettings
    {
        public string Size { get; set; } = "medium";
        public string Color { get; set; } = "#000000";
        public bool IncludeLogo { get; set; } = true;
    }

    public class EmailCampaign
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public int RecipientCount { get; set; }
    }

    public class SMSTemplate
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public class SentEmail
    {
        public string Id { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Recipient { get; set; } = "";
        public DateTime SentDate { get; set; }
        public string Status { get; set; } = "";
    }

    public class EmailTemplate
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public class EventAnalytics
    {
        public List<string> EventNames { get; set; } = new();
        public List<int> GuestCounts { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
    }

    public class CheckInReports
    {
        public List<CheckIn> RecentCheckins { get; set; } = new();
        public List<string> TimeSlots { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
    }

    public class ExportOption
    {
        public string Name { get; set; } = "";
        public string Format { get; set; } = "";
        public string Description { get; set; } = "";
    }
} 