using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Constants;
using System.Security.Claims;

namespace GiaNguyenCheck.Controllers.API
{
    /// <summary>
    /// Controller dành riêng cho System Admin
    /// Chỉ SystemAdmin mới được truy cập các API này
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppConstants.Roles.SystemAdmin)]
    public class SystemAdminController : ControllerBase
    {
        private readonly ITenantManagementService _tenantManagementService;
        private readonly ILogger<SystemAdminController> _logger;

        public SystemAdminController(
            ITenantManagementService tenantManagementService,
            ILogger<SystemAdminController> logger)
        {
            _tenantManagementService = tenantManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo tenant mới và cấp bản Free
        /// Chỉ SystemAdmin mới được sử dụng API này
        /// </summary>
        /// <param name="dto">Thông tin tenant và admin</param>
        /// <returns>Thông tin tenant đã tạo</returns>
        [HttpPost("tenants")]
        public async Task<ActionResult<ApiResponse<TenantDto>>> CreateTenant([FromBody] RegisterTenantDto dto)
        {
            try
            {
                _logger.LogInformation("SystemAdmin {UserId} đang tạo tenant mới: {TenantName}", 
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value, dto.TenantName);

                var result = await _tenantManagementService.CreateTenantAsync(dto);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                _logger.LogInformation("Đã tạo thành công tenant {TenantId} với admin {AdminEmail}", 
                    result.Data?.Id, dto.AdminEmail);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tenant");
                return StatusCode(500, ApiResponse<TenantDto>.ErrorResult("Có lỗi xảy ra khi tạo tenant"));
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả tenant trong hệ thống
        /// </summary>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="pageSize">Số lượng tenant mỗi trang</param>
        /// <param name="search">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách tenant phân trang</returns>
        [HttpGet("tenants")]
        public async Task<ActionResult<ApiResponse<PagedResult<TenantDto>>>> GetTenants(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            try
            {
                if (pageSize > 100) pageSize = 100; // Giới hạn tối đa

                var result = await _tenantManagementService.GetTenantsAsync(page, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tenant");
                return StatusCode(500, ApiResponse<PagedResult<TenantDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách tenant"));
            }
        }

        /// <summary>
        /// Tạo mã mời cho user trong tenant (TenantAdmin sử dụng)
        /// SystemAdmin có thể tạo mã mời cho bất kỳ tenant nào
        /// </summary>
        /// <param name="tenantId">ID tenant</param>
        /// <param name="dto">Thông tin mã mời</param>
        /// <returns>Thông tin mã mời đã tạo</returns>
        [HttpPost("tenants/{tenantId}/invitations")]
        public async Task<ActionResult<ApiResponse<InvitationDto>>> CreateInvitation(
            int tenantId,
            [FromBody] CreateInvitationDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                _logger.LogInformation("SystemAdmin {UserId} đang tạo mã mời cho email {Email} trong tenant {TenantId}", 
                    userId, dto.Email, tenantId);

                var result = await _tenantManagementService.CreateInvitationAsync(dto, tenantId, userId);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                _logger.LogInformation("Đã tạo thành công mã mời {InvitationCode} cho {Email}", 
                    result.Data?.Code, dto.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo mã mời");
                return StatusCode(500, ApiResponse<InvitationDto>.ErrorResult("Có lỗi xảy ra khi tạo mã mời"));
            }
        }

        /// <summary>
        /// Validate mã mời (dùng khi đăng ký user mới)
        /// </summary>
        /// <param name="code">Mã mời</param>
        /// <returns>Thông tin mã mời nếu hợp lệ</returns>
        [HttpGet("invitations/{code}/validate")]
        [AllowAnonymous] // Cho phép anonymous để validate khi đăng ký
        public async Task<ActionResult<ApiResponse<InvitationDto>>> ValidateInvitation(string code)
        {
            try
            {
                var result = await _tenantManagementService.ValidateInvitationAsync(code);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate mã mời {Code}", code);
                return StatusCode(500, ApiResponse<InvitationDto>.ErrorResult("Có lỗi xảy ra khi kiểm tra mã mời"));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái tenant (kích hoạt/vô hiệu hóa)
        /// </summary>
        /// <param name="tenantId">ID tenant</param>
        /// <param name="isActive">Trạng thái mới</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPatch("tenants/{tenantId}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTenantStatus(
            int tenantId,
            [FromBody] bool isActive)
        {
            try
            {
                // TODO: Implement update tenant status logic
                _logger.LogInformation("SystemAdmin đang cập nhật trạng thái tenant {TenantId} thành {Status}", 
                    tenantId, isActive ? "Active" : "Inactive");

                // For now, return success
                return Ok(ApiResponse<bool>.SuccessResult(true, "Cập nhật trạng thái thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái tenant {TenantId}", tenantId);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi cập nhật trạng thái"));
            }
        }

        /// <summary>
        /// Cập nhật gói dịch vụ cho tenant
        /// </summary>
        /// <param name="tenantId">ID tenant</param>
        /// <param name="plan">Gói dịch vụ mới</param>
        /// <param name="expiryDate">Ngày hết hạn (optional)</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPatch("tenants/{tenantId}/plan")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTenantPlan(
            int tenantId,
            [FromBody] UpdatePlanDto dto)
        {
            try
            {
                // TODO: Implement update tenant plan logic
                _logger.LogInformation("SystemAdmin đang cập nhật gói dịch vụ tenant {TenantId} thành {Plan}", 
                    tenantId, dto.Plan);

                // For now, return success
                return Ok(ApiResponse<bool>.SuccessResult(true, "Cập nhật gói dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật gói dịch vụ tenant {TenantId}", tenantId);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi cập nhật gói dịch vụ"));
            }
        }

        /// <summary>
        /// Lấy thống kê tổng quan hệ thống
        /// </summary>
        /// <returns>Thống kê hệ thống</returns>
        [HttpGet("dashboard/stats")]
        public async Task<ActionResult<ApiResponse<SystemStatsDto>>> GetSystemStats()
        {
            try
            {
                // TODO: Implement system stats logic
                var stats = new SystemStatsDto
                {
                    TotalTenants = 0,
                    ActiveTenants = 0,
                    TotalUsers = 0,
                    TotalEvents = 0,
                    TotalCheckIns = 0,
                    TotalRevenue = 0,
                    // Add more stats as needed
                };

                return Ok(ApiResponse<SystemStatsDto>.SuccessResult(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê hệ thống");
                return StatusCode(500, ApiResponse<SystemStatsDto>.ErrorResult("Có lỗi xảy ra khi lấy thống kê"));
            }
        }
    }

    /// <summary>
    /// DTO cho cập nhật gói dịch vụ
    /// </summary>
    public class UpdatePlanDto
    {
        public string Plan { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê hệ thống
    /// </summary>
    public class SystemStatsDto
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int TotalUsers { get; set; }
        public int TotalEvents { get; set; }
        public int TotalCheckIns { get; set; }
        public decimal TotalRevenue { get; set; }
        public int FreePlanTenants { get; set; }
        public int BasicPlanTenants { get; set; }
        public int ProPlanTenants { get; set; }
        public int EnterprisePlanTenants { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
} 