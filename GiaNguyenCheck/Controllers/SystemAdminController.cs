using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiaNguyenCheck.Services;
using GiaNguyenCheck.DTOs.CommonDTOs;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    [Route("SystemAdmin/[action]")]
    public class SystemAdminController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly IPaymentService _paymentService;
        private readonly IDashboardService _dashboardService;

        public SystemAdminController(
            ITenantService tenantService,
            IUserService userService,
            IEventService eventService,
            IPaymentService paymentService,
            IDashboardService dashboardService)
        {
            _tenantService = tenantService;
            _userService = userService;
            _eventService = eventService;
            _paymentService = paymentService;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "System Dashboard";
            ViewData["Role"] = "SystemAdmin";
            
            var model = new SystemAdminDashboardViewModel
            {
                TotalCustomers = await _tenantService.GetTotalTenantsAsync(),
                MonthlyRevenue = await _paymentService.GetMonthlyRevenueAsync(),
                ChurnRate = await _paymentService.GetChurnRateAsync(),
                SystemHealth = await _dashboardService.GetSystemHealthAsync(),
                RecentCustomers = await _tenantService.GetRecentTenantsAsync(5),
                RevenueChart = await _paymentService.GetRevenueChartDataAsync(),
                PlatformMetrics = await _dashboardService.GetPlatformMetricsAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> CustomerManagement()
        {
            ViewData["Title"] = "Customer Management";
            ViewData["Role"] = "SystemAdmin";
            
            var model = new CustomerManagementViewModel
            {
                AllCustomers = await _tenantService.GetAllTenantsAsync(),
                PendingApprovals = await _tenantService.GetPendingApprovalsAsync(),
                SubscriptionOverview = await _tenantService.GetSubscriptionOverviewAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> RevenueManagement()
        {
            ViewData["Title"] = "Revenue Management";
            ViewData["Role"] = "SystemAdmin";
            
            var model = new RevenueManagementViewModel
            {
                PaymentHistory = await _paymentService.GetPaymentHistoryAsync(),
                RevenueByPlan = await _paymentService.GetRevenueByPlanAsync(),
                MonthlyRecurringRevenue = await _paymentService.GetMonthlyRecurringRevenueAsync(),
                ChurnRate = await _paymentService.GetChurnRateAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> TenantManagement()
        {
            ViewData["Title"] = "Tenant Management";
            ViewData["Role"] = "SystemAdmin";
            
            var model = new TenantManagementViewModel
            {
                ActiveTenants = await _tenantService.GetActiveTenantsAsync(),
                SuspendedTenants = await _tenantService.GetSuspendedTenantsAsync(),
                StorageUsage = await _tenantService.GetStorageUsageAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> PlatformAnalytics()
        {
            ViewData["Title"] = "Platform Analytics";
            ViewData["Role"] = "SystemAdmin";
            
            var model = new PlatformAnalyticsViewModel
            {
                TotalEvents = await _eventService.GetTotalEventsAsync(),
                TotalCheckins = await _eventService.GetTotalCheckinsAsync(),
                SystemPerformance = await _dashboardService.GetSystemPerformanceAsync(),
                UserGrowth = await _userService.GetUserGrowthAsync()
            };
            
            return View(model);
        }

        public IActionResult SystemSettings()
        {
            ViewData["Title"] = "System Settings";
            ViewData["Role"] = "SystemAdmin";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveTenant(string tenantId)
        {
            await _tenantService.ApproveTenantAsync(tenantId);
            return RedirectToAction(nameof(CustomerManagement));
        }

        [HttpPost]
        public async Task<IActionResult> SuspendTenant(string tenantId)
        {
            await _tenantService.SuspendTenantAsync(tenantId);
            return RedirectToAction(nameof(TenantManagement));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePricingPlan(PricingPlanViewModel model)
        {
            await _tenantService.UpdatePricingPlanAsync(model);
            return RedirectToAction(nameof(SystemSettings));
        }
    }

    public class SystemAdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal ChurnRate { get; set; }
        public SystemHealth SystemHealth { get; set; } = new();
        public List<Tenant> RecentCustomers { get; set; } = new();
        public RevenueChartData RevenueChart { get; set; } = new();
        public PlatformMetrics PlatformMetrics { get; set; } = new();
    }

    public class CustomerManagementViewModel
    {
        public List<Tenant> AllCustomers { get; set; } = new();
        public List<Tenant> PendingApprovals { get; set; } = new();
        public SubscriptionOverview SubscriptionOverview { get; set; } = new();
    }

    public class RevenueManagementViewModel
    {
        public List<Payment> PaymentHistory { get; set; } = new();
        public RevenueByPlan RevenueByPlan { get; set; } = new();
        public decimal MonthlyRecurringRevenue { get; set; }
        public decimal ChurnRate { get; set; }
    }

    public class TenantManagementViewModel
    {
        public List<Tenant> ActiveTenants { get; set; } = new();
        public List<Tenant> SuspendedTenants { get; set; } = new();
        public StorageUsage StorageUsage { get; set; } = new();
    }

    public class PlatformAnalyticsViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalCheckins { get; set; }
        public SystemPerformance SystemPerformance { get; set; } = new();
        public UserGrowth UserGrowth { get; set; } = new();
    }

    public class PricingPlanViewModel
    {
        public string PlanId { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int MaxEvents { get; set; }
        public int MaxGuests { get; set; }
    }

    public class SystemHealth
    {
        public string Status { get; set; } = "Healthy";
        public decimal CpuUsage { get; set; }
        public decimal MemoryUsage { get; set; }
        public decimal DiskUsage { get; set; }
    }

    public class RevenueChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
    }

    public class PlatformMetrics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal ConversionRate { get; set; }
    }

    public class SubscriptionOverview
    {
        public int FreeCount { get; set; }
        public int BasicCount { get; set; }
        public int ProCount { get; set; }
        public int EnterpriseCount { get; set; }
    }

    public class RevenueByPlan
    {
        public decimal FreeRevenue { get; set; }
        public decimal BasicRevenue { get; set; }
        public decimal ProRevenue { get; set; }
        public decimal EnterpriseRevenue { get; set; }
    }

    public class StorageUsage
    {
        public decimal TotalStorage { get; set; }
        public decimal UsedStorage { get; set; }
        public decimal AvailableStorage { get; set; }
    }

    public class SystemPerformance
    {
        public decimal ResponseTime { get; set; }
        public decimal Uptime { get; set; }
        public int ErrorRate { get; set; }
    }

    public class UserGrowth
    {
        public List<string> Months { get; set; } = new();
        public List<int> NewUsers { get; set; } = new();
        public List<int> ActiveUsers { get; set; } = new();
    }
} 