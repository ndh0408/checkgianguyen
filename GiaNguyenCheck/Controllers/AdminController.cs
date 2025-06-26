using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiaNguyenCheck.Services;
using GiaNguyenCheck.DTOs.CommonDTOs;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    public class AdminController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly IPaymentService _paymentService;
        private readonly IDashboardService _dashboardService;

        public AdminController(
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
            ViewData["Title"] = "Tổng quan hệ thống";
            
            var model = new SystemAdminDashboardViewModel
            {
                TotalTenants = await _tenantService.GetTotalTenantsAsync(),
                TotalUsers = await _userService.GetTotalUsersAsync(),
                TotalEvents = await _eventService.GetTotalEventsAsync(),
                TotalRevenue = await _paymentService.GetTotalRevenueAsync(),
                RecentTenants = await _tenantService.GetRecentTenantsAsync(5),
                SystemMetrics = await _dashboardService.GetSystemMetricsAsync()
            };
            
            return View(model);
        }

        public async Task<IActionResult> Tenants()
        {
            ViewData["Title"] = "Quản lý Tenant";
            var tenants = await _tenantService.GetAllTenantsAsync();
            return View(tenants);
        }

        public async Task<IActionResult> Users()
        {
            ViewData["Title"] = "Quản lý người dùng";
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Plans()
        {
            ViewData["Title"] = "Quản lý gói dịch vụ";
            var plans = await _tenantService.GetAllPlansAsync();
            return View(plans);
        }

        public async Task<IActionResult> Metrics()
        {
            ViewData["Title"] = "Thống kê hệ thống";
            var metrics = await _dashboardService.GetSystemMetricsAsync();
            return View(metrics);
        }

        public IActionResult Settings()
        {
            ViewData["Title"] = "Cài đặt hệ thống";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenant(CreateTenantViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _tenantService.CreateTenantAsync(model);
                return RedirectToAction(nameof(Tenants));
            }
            return View("Tenants", await _tenantService.GetAllTenantsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string userId, string role)
        {
            await _userService.UpdateUserRoleAsync(userId, role);
            return RedirectToAction(nameof(Users));
        }
    }

    public class SystemAdminDashboardViewModel
    {
        public int TotalTenants { get; set; }
        public int TotalUsers { get; set; }
        public int TotalEvents { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Tenant> RecentTenants { get; set; } = new();
        public SystemMetrics SystemMetrics { get; set; } = new();
    }

    public class CreateTenantViewModel
    {
        public string Name { get; set; } = "";
        public string Subdomain { get; set; } = "";
        public string AdminEmail { get; set; } = "";
        public string PlanId { get; set; } = "";
    }
} 