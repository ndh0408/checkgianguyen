using System.Diagnostics;
using GiaNguyenCheck.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiaNguyenCheck.Services;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, ITenantService tenantService, IUserService userService)
        {
            _logger = logger;
            _tenantService = tenantService;
            _userService = userService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewData["Title"] = "GiaNguyenCheck - Hệ thống quản lý check-in sự kiện đa tenant";
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "Giới thiệu";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Features()
        {
            ViewData["Title"] = "Tính năng - GiaNguyenCheck";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Pricing()
        {
            ViewData["Title"] = "Bảng giá - GiaNguyenCheck";
            var plans = _tenantService.GetAllPlansAsync().Result;
            return View(plans);
        }

        [AllowAnonymous]
        public IActionResult Contact()
        {
            ViewData["Title"] = "Liên hệ - GiaNguyenCheck";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            ViewData["Title"] = "Chính sách bảo mật - GiaNguyenCheck";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Terms()
        {
            ViewData["Title"] = "Điều khoản sử dụng - GiaNguyenCheck";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewData["Title"] = "Đăng ký - GiaNguyenCheck";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            ViewData["Title"] = "Lỗi - GiaNguyenCheck";
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Redirect based on user role
            return user.Role switch
            {
                UserRole.SystemAdmin => RedirectToAction("Dashboard", "SystemAdmin"),
                UserRole.TenantAdmin => RedirectToAction("Dashboard", "TenantAdmin"),
                UserRole.EventManager => RedirectToAction("Dashboard", "EventManager"),
                UserRole.Staff => RedirectToAction("CheckInScanner", "Staff"),
                UserRole.Viewer => RedirectToAction("Reports", "Viewer"),
                _ => RedirectToAction("Index")
            };
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
