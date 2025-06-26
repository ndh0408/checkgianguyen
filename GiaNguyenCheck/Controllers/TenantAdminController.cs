using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiaNguyenCheck.Services;
using GiaNguyenCheck.DTOs.CommonDTOs;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.Controllers
{
    [Authorize(Roles = "TenantAdmin")]
    [Route("TenantAdmin/[action]")]
    public class TenantAdminController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly IPaymentService _paymentService;
        private readonly IDashboardService _dashboardService;
        private readonly ITenantProvider _tenantProvider;

        public TenantAdminController(
            ITenantService tenantService,
            IUserService userService,
            IEventService eventService,
            IPaymentService paymentService,
            IDashboardService dashboardService,
            ITenantProvider tenantProvider)
        {
            _tenantService = tenantService;
            _userService = userService;
            _eventService = eventService;
            _paymentService = paymentService;
            _dashboardService = dashboardService;
            _tenantProvider = tenantProvider;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Company Dashboard";
            ViewData["Role"] = "TenantAdmin";
            
            var currentTenant = _tenantProvider.GetCurrentTenant();
            var model = new TenantAdminDashboardViewModel
            {
                CompanyName = currentTenant?.Name ?? "",
                TotalEmployees = await _userService.GetTotalEmployeesByTenantAsync(currentTenant?.Id),
                TotalEvents = await _eventService.GetTotalEventsByTenantAsync(currentTenant?.Id),
                TotalGuests = await _eventService.GetTotalGuestsByTenantAsync(currentTenant?.Id),
                SuccessRate = await _eventService.GetSuccessRateByTenantAsync(currentTenant?.Id),
                CurrentPlan = currentTenant?.Plan,
                UsageStatistics = await _tenantService.GetUsageStatisticsAsync(currentTenant?.Id),
                RecentEvents = await _eventService.GetRecentEventsByTenantAsync(currentTenant?.Id, 5),
                EmployeeActivity = await _userService.GetEmployeeActivityAsync(currentTenant?.Id)
            };
            
            return View(model);
        }

        public async Task<IActionResult> EmployeeManagement()
        {
            ViewData["Title"] = "Employee Management";
            ViewData["Role"] = "TenantAdmin";
            
            var currentTenant = _tenantProvider.GetCurrentTenant();
            var model = new EmployeeManagementViewModel
            {
                AllEmployees = await _userService.GetEmployeesByTenantAsync(currentTenant?.Id),
                RolesAssignment = await _userService.GetRolesAssignmentAsync(currentTenant?.Id),
                Permissions = await _userService.GetPermissionsAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> EventsOverview()
        {
            ViewData["Title"] = "Events Overview";
            ViewData["Role"] = "TenantAdmin";
            
            var currentTenant = _tenantProvider.GetCurrentTenant();
            var model = new EventsOverviewViewModel
            {
                AllCompanyEvents = await _eventService.GetEventsByTenantAsync(currentTenant?.Id),
                EventCalendar = await _eventService.GetEventCalendarAsync(currentTenant?.Id),
                ResourceAllocation = await _eventService.GetResourceAllocationAsync(currentTenant?.Id)
            };
            
            return View(model);
        }

        public async Task<IActionResult> BillingSubscription()
        {
            ViewData["Title"] = "Billing & Subscription";
            ViewData["Role"] = "TenantAdmin";
            
            var currentTenant = _tenantProvider.GetCurrentTenant();
            var model = new BillingSubscriptionViewModel
            {
                CurrentPlan = currentTenant?.Plan,
                UsageStatistics = await _tenantService.GetUsageStatisticsAsync(currentTenant?.Id),
                PaymentHistory = await _paymentService.GetPaymentHistoryByTenantAsync(currentTenant?.Id),
                AvailablePlans = await _tenantService.GetAvailablePlansAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> CompanyAnalytics()
        {
            ViewData["Title"] = "Company Analytics";
            ViewData["Role"] = "TenantAdmin";
            
            var currentTenant = _tenantProvider.GetCurrentTenant();
            var model = new CompanyAnalyticsViewModel
            {
                TotalEvents = await _eventService.GetTotalEventsByTenantAsync(currentTenant?.Id),
                TotalGuests = await _eventService.GetTotalGuestsByTenantAsync(currentTenant?.Id),
                SuccessRate = await _eventService.GetSuccessRateByTenantAsync(currentTenant?.Id),
                EventTrends = await _eventService.GetEventTrendsAsync(currentTenant?.Id),
                GuestAnalytics = await _eventService.GetGuestAnalyticsAsync(currentTenant?.Id),
                EmployeePerformance = await _userService.GetEmployeePerformanceAsync(currentTenant?.Id)
            };
            
            return View(model);
        }

        public async Task<IActionResult> CompanySettings()
        {
            ViewData["Title"] = "Company Settings";
            ViewData["Role"] = "TenantAdmin";
            
            var currentTenant = _tenantProvider.GetCurrentTenant();
            var model = new CompanySettingsViewModel
            {
                CompanyProfile = currentTenant,
                Branding = await _tenantService.GetBrandingAsync(currentTenant?.Id),
                Integrations = await _tenantService.GetIntegrationsAsync(currentTenant?.Id)
            };
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeViewModel model)
        {
            var currentTenant = _tenantProvider.GetCurrentTenant();
            await _userService.CreateEmployeeAsync(model, currentTenant?.Id);
            return RedirectToAction(nameof(EmployeeManagement));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployeeRole(string userId, string role)
        {
            await _userService.UpdateEmployeeRoleAsync(userId, role);
            return RedirectToAction(nameof(EmployeeManagement));
        }

        [HttpPost]
        public async Task<IActionResult> UpgradePlan(string planId)
        {
            var currentTenant = _tenantProvider.GetCurrentTenant();
            await _tenantService.UpgradePlanAsync(currentTenant?.Id, planId);
            return RedirectToAction(nameof(BillingSubscription));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCompanyProfile(CompanyProfileViewModel model)
        {
            var currentTenant = _tenantProvider.GetCurrentTenant();
            await _tenantService.UpdateCompanyProfileAsync(currentTenant?.Id, model);
            return RedirectToAction(nameof(CompanySettings));
        }
    }

    public class TenantAdminDashboardViewModel
    {
        public string CompanyName { get; set; } = "";
        public int TotalEmployees { get; set; }
        public int TotalEvents { get; set; }
        public int TotalGuests { get; set; }
        public decimal SuccessRate { get; set; }
        public Plan? CurrentPlan { get; set; }
        public UsageStatistics UsageStatistics { get; set; } = new();
        public List<Event> RecentEvents { get; set; } = new();
        public EmployeeActivity EmployeeActivity { get; set; } = new();
    }

    public class EmployeeManagementViewModel
    {
        public List<User> AllEmployees { get; set; } = new();
        public RolesAssignment RolesAssignment { get; set; } = new();
        public List<Permission> Permissions { get; set; } = new();
    }

    public class EventsOverviewViewModel
    {
        public List<Event> AllCompanyEvents { get; set; } = new();
        public EventCalendar EventCalendar { get; set; } = new();
        public ResourceAllocation ResourceAllocation { get; set; } = new();
    }

    public class BillingSubscriptionViewModel
    {
        public Plan? CurrentPlan { get; set; }
        public UsageStatistics UsageStatistics { get; set; } = new();
        public List<Payment> PaymentHistory { get; set; } = new();
        public List<Plan> AvailablePlans { get; set; } = new();
    }

    public class CompanyAnalyticsViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalGuests { get; set; }
        public decimal SuccessRate { get; set; }
        public EventTrends EventTrends { get; set; } = new();
        public GuestAnalytics GuestAnalytics { get; set; } = new();
        public EmployeePerformance EmployeePerformance { get; set; } = new();
    }

    public class CompanySettingsViewModel
    {
        public Tenant? CompanyProfile { get; set; }
        public Branding Branding { get; set; } = new();
        public List<Integration> Integrations { get; set; } = new();
    }

    public class CreateEmployeeViewModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Role { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class CompanyProfileViewModel
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public string PrimaryColor { get; set; } = "";
        public string SecondaryColor { get; set; } = "";
    }

    public class UsageStatistics
    {
        public int EventsUsed { get; set; }
        public int EventsLimit { get; set; }
        public int GuestsUsed { get; set; }
        public int GuestsLimit { get; set; }
        public decimal StorageUsed { get; set; }
        public decimal StorageLimit { get; set; }
    }

    public class EmployeeActivity
    {
        public List<string> Dates { get; set; } = new();
        public List<int> ActiveEmployees { get; set; } = new();
    }

    public class RolesAssignment
    {
        public int EventManagerCount { get; set; }
        public int StaffCount { get; set; }
        public int ViewerCount { get; set; }
    }

    public class EventCalendar
    {
        public List<Event> UpcomingEvents { get; set; } = new();
        public List<Event> PastEvents { get; set; } = new();
    }

    public class ResourceAllocation
    {
        public int AssignedStaff { get; set; }
        public int AvailableStaff { get; set; }
        public List<Event> EventsNeedingStaff { get; set; } = new();
    }

    public class EventTrends
    {
        public List<string> Months { get; set; } = new();
        public List<int> EventCounts { get; set; } = new();
        public List<int> GuestCounts { get; set; } = new();
    }

    public class GuestAnalytics
    {
        public int TotalInvited { get; set; }
        public int TotalAttended { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    public class EmployeePerformance
    {
        public List<User> TopPerformers { get; set; } = new();
        public List<Event> RecentCheckins { get; set; } = new();
    }

    public class Branding
    {
        public string LogoUrl { get; set; } = "";
        public string PrimaryColor { get; set; } = "";
        public string SecondaryColor { get; set; } = "";
        public string FontFamily { get; set; } = "";
    }

    public class Integration
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsEnabled { get; set; }
    }

    public class Permission
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }
} 