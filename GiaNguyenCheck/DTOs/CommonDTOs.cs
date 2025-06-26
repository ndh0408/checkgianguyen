using System.ComponentModel.DataAnnotations;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.DTOs
{
    /// <summary>
    /// DTO cho thông tin tenant
    /// </summary>
    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public string Subdomain { get; set; } = string.Empty;
        public string CurrentPlan { get; set; } = string.Empty;
        public DateTime? PlanExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Thông tin thống kê
        public int TotalEvents { get; set; }
        public int TotalUsers { get; set; }
        public int TotalGuests { get; set; }
        public int TotalCheckIns { get; set; }
    }
    
    /// <summary>
    /// DTO cho tạo tenant
    /// </summary>
    public class CreateTenantDto
    {
        [Required(ErrorMessage = "Tên tổ chức là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên tổ chức không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Subdomain là bắt buộc")]
        [StringLength(50, ErrorMessage = "Subdomain không được vượt quá 50 ký tự")]
        public string Subdomain { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email admin là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string AdminEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tên admin là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên admin không được vượt quá 100 ký tự")]
        public string AdminFirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Họ admin là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ admin không được vượt quá 100 ký tự")]
        public string AdminLastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu admin là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string AdminPassword { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
    }
    
    /// <summary>
    /// DTO cho cập nhật tenant
    /// </summary>
    public class UpdateTenantDto
    {
        [StringLength(200, ErrorMessage = "Tên tổ chức không được vượt quá 200 ký tự")]
        public string? Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50, ErrorMessage = "Subdomain không được vượt quá 50 ký tự")]
        public string? Subdomain { get; set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
    }
    
    /// <summary>
    /// DTO cho tạo/cập nhật tenant
    /// </summary>
    public class CreateUpdateTenantDto
    {
        [Required(ErrorMessage = "Tên tổ chức là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên tổ chức không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(200)]
        public string? Website { get; set; }
        
        [StringLength(200)]
        public string? LogoUrl { get; set; }
    }
    
    /// <summary>
    /// DTO cho sự kiện
    /// </summary>
    public class EventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CheckInStartTime { get; set; }
        public DateTime CheckInEndTime { get; set; }
        public string? BannerImageUrl { get; set; }
        public EventStatus Status { get; set; }
        public int MaxAttendees { get; set; }
        public bool AllowMultipleCheckIn { get; set; }
        public int TenantId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Thống kê
        public int TotalGuests { get; set; }
        public int TotalCheckedIn { get; set; }
        
        // Thông tin người tạo
        public string CreatedByUserName { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO cho tạo sự kiện
    /// </summary>
    public class CreateEventDto
    {
        [Required(ErrorMessage = "Tên sự kiện là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sự kiện không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Địa điểm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa điểm không được vượt quá 200 ký tự")]
        public string Location { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public DateTime StartTime { get; set; }
        
        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public DateTime EndTime { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [StringLength(200)]
        public string? OrganizerName { get; set; }
        
        [StringLength(20)]
        public string? OrganizerPhone { get; set; }
        
        [EmailAddress]
        public string? OrganizerEmail { get; set; }
        
        public int? MaxGuests { get; set; }
        
        public bool IsPublic { get; set; } = true;
    }
    
    /// <summary>
    /// DTO cho cập nhật sự kiện
    /// </summary>
    public class UpdateEventDto
    {
        [Required(ErrorMessage = "Tên sự kiện là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sự kiện không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Địa điểm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa điểm không được vượt quá 200 ký tự")]
        public string Location { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public DateTime StartTime { get; set; }
        
        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public DateTime EndTime { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [StringLength(200)]
        public string? OrganizerName { get; set; }
        
        [StringLength(20)]
        public string? OrganizerPhone { get; set; }
        
        [EmailAddress]
        public string? OrganizerEmail { get; set; }
        
        public int? MaxGuests { get; set; }
        
        public bool IsPublic { get; set; } = true;
        
        public EventStatus Status { get; set; }
    }
    
    /// <summary>
    /// DTO cho tạo/cập nhật sự kiện
    /// </summary>
    public class CreateUpdateEventDto
    {
        [Required(ErrorMessage = "Tên sự kiện là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sự kiện không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Địa điểm là bắt buộc")]
        [StringLength(300, ErrorMessage = "Địa điểm không được vượt quá 300 ký tự")]
        public string Location { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public DateTime StartTime { get; set; }
        
        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public DateTime EndTime { get; set; }
        
        [Required(ErrorMessage = "Thời gian bắt đầu check-in là bắt buộc")]
        public DateTime CheckInStartTime { get; set; }
        
        [Required(ErrorMessage = "Thời gian kết thúc check-in là bắt buộc")]
        public DateTime CheckInEndTime { get; set; }
        
        [StringLength(300)]
        public string? BannerImageUrl { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng khách tối đa phải >= 0")]
        public int MaxAttendees { get; set; } = 0;
        
        public bool AllowMultipleCheckIn { get; set; } = false;
        
        public string? Settings { get; set; }
    }
    
    /// <summary>
    /// DTO cho khách mời
    /// </summary>
    public class GuestDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public string? AvatarUrl { get; set; }
        public GuestType Type { get; set; }
        public string? TableNumber { get; set; }
        public string? Notes { get; set; }
        public InvitationStatus InvitationStatus { get; set; }
        public DateTime? InvitationSentAt { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime? FirstCheckInTime { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Thông tin sự kiện
        public string EventName { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO cho cập nhật khách mời
    /// </summary>
    public class UpdateGuestDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(50)]
        public string? Group { get; set; }
        
        [StringLength(20)]
        public string? TableNumber { get; set; }
        
        public string? Notes { get; set; }
        
        [Url]
        [StringLength(500)]
        public string? AvatarUrl { get; set; }
        
        public bool IsVIP { get; set; }
    }
    
    /// <summary>
    /// DTO cho tạo/cập nhật khách mời
    /// </summary>
    public class CreateUpdateGuestDto
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ không được vượt quá 100 ký tự")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Company { get; set; }
        
        [StringLength(100)]
        public string? Position { get; set; }
        
        [StringLength(300)]
        public string? AvatarUrl { get; set; }
        
        public GuestType Type { get; set; } = GuestType.Regular;
        
        [StringLength(20)]
        public string? TableNumber { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// DTO cho import khách hàng loạt
    /// </summary>
    public class BulkImportGuestDto
    {
        public int EventId { get; set; }
        public List<CreateUpdateGuestDto> Guests { get; set; } = new List<CreateUpdateGuestDto>();
    }

    /// <summary>
    /// DTO cho check-in
    /// </summary>
    public class CheckInDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int GuestId { get; set; }
        public string? QRCode { get; set; }
        public string? DeviceId { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public CheckInStatus Status { get; set; }
        public CheckInType Type { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? CheckedInByUserId { get; set; }
        public int? CheckedOutByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê check-in
    /// </summary>
    public class CheckInStatsDto
    {
        public int TotalGuests { get; set; }
        public int TotalCheckedIn { get; set; }
        public int TotalCheckedOut { get; set; }
        public int PendingCheckIn { get; set; }
        public double CheckInRate { get; set; }
        public double CheckOutRate { get; set; }
        public DateTime? FirstCheckInTime { get; set; }
        public DateTime? LastCheckInTime { get; set; }
        public Dictionary<string, int> CheckInsByHour { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CheckInsByType { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// DTO cho thực hiện check-in
    /// </summary>
    public class PerformCheckInDto
    {
        [Required(ErrorMessage = "Mã QR là bắt buộc")]
        public string QRCode { get; set; } = string.Empty;
        
        public CheckInType Type { get; set; } = CheckInType.QRCode;
        
        public string? DeviceInfo { get; set; }
        
        public string? Location { get; set; }
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cho kết quả check-in
    /// </summary>
    public class CheckInResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CheckInDto? CheckIn { get; set; }
        public GuestDto? Guest { get; set; }
        public EventDto? Event { get; set; }
    }

    /// <summary>
    /// DTO cho kết quả phân trang
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// DTO cho kết quả phân trang (legacy)
    /// </summary>
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// DTO cho API response
    /// </summary>
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        // Static Success methods
        public static ApiResponse<T> Success(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        // Static Error methods
        public static ApiResponse<T> Error(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// DTO cho kết quả import
    /// </summary>
    public class ImportResultDto
    {
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public string? Message { get; set; }
    }

    /// <summary>
    /// DTO cho check-in offline
    /// </summary>
    public class OfflineCheckInDto
    {
        public string QRCode { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
        public CheckInType Type { get; set; }
        public string? DeviceInfo { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public string? LocalId { get; set; }
    }

    /// <summary>
    /// DTO cho trạng thái giới hạn dịch vụ
    /// </summary>
    public class ServiceLimitStatusDto
    {
        public bool CanCreateEvent { get; set; }
        public bool CanAddGuest { get; set; }
        public bool HasReachedConcurrentCheckInLimit { get; set; }
        public bool IsWithinLimits { get; set; }
        public int EventsUsed { get; set; }
        public int EventsLimit { get; set; }
        public int CurrentEvents { get; set; }
        public int MaxEvents { get; set; }
        public int GuestsUsed { get; set; }
        public int GuestsLimit { get; set; }
        public int CurrentGuests { get; set; }
        public int MaxGuests { get; set; }
        public int ConcurrentCheckInsUsed { get; set; }
        public int ConcurrentCheckInsLimit { get; set; }
        public double EventsUsagePercentage { get; set; }
        public double GuestsUsagePercentage { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê sự kiện
    /// </summary>
    public class EventStatsDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TotalGuests { get; set; }
        public int TotalCheckedIn { get; set; }
        public int TotalCheckedOut { get; set; }
        public int VipGuests { get; set; }
        public int RegularGuests { get; set; }
        public int VipCheckedIn { get; set; }
        public int RegularCheckedIn { get; set; }
        public DateTime? LastCheckInTime { get; set; }
        public Dictionary<string, int> CheckInsByHour { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CheckInsByType { get; set; } = new Dictionary<string, int>();
        public double CheckInRate => TotalGuests > 0 ? (double)TotalCheckedIn / TotalGuests * 100 : 0;
        
        // Thông tin thời gian
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// DTO cho thanh toán
    /// </summary>
    public class PaymentDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string PaymentMethod { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Status { get; set; } = "Pending";
        public string TransactionId { get; set; } = string.Empty;
        public string? ExternalTransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// DTO cho tạo thanh toán
    /// </summary>
    public class CreatePaymentDto
    {
        [Required]
        public int TenantId { get; set; }
        
        [Required]
        public string PlanName { get; set; } = string.Empty;
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? DurationMonths { get; set; } = 1;
    }

    /// <summary>
    /// DTO cho thống kê dashboard
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalEvents { get; set; }
        public int TotalGuests { get; set; }
        public int TotalCheckedIn { get; set; }
        public int TotalTenants { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveEvents { get; set; }
        public int TodayCheckIns { get; set; }
        public List<ChartDataPoint> CheckInTrend { get; set; } = new();
        public List<ChartDataPoint> RevenueTrend { get; set; } = new();
    }

    /// <summary>
    /// DTO cho user
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        
        // Thông tin bổ sung
        public string? Position { get; set; }
    }

    /// <summary>
    /// DTO cho tạo/cập nhật user
    /// </summary>
    public class CreateUpdateUserDto
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(300)]
        public string? AvatarUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public List<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO cho tạo khách mời đơn giản
    /// </summary>
    public class CreateGuestDto
    {
        [Required(ErrorMessage = "Tên khách mời là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Họ khách mời là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ không được vượt quá 100 ký tự")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [Required(ErrorMessage = "Sự kiện là bắt buộc")]
        public int EventId { get; set; }
        
        [StringLength(50)]
        public string? Group { get; set; } // VIP, Thường, etc.
        
        [StringLength(50)]
        public string? TableNumber { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(500)]
        public string? AvatarUrl { get; set; }
        
        public bool IsVIP { get; set; } = false;
        
        public bool SendInvitation { get; set; } = true;
    }

    /// <summary>
    /// DTO cho thống kê check-in theo xu hướng
    /// </summary>
    public class CheckInTrendDto
    {
        public DateTime Date { get; set; }
        public int CheckInCount { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê check-in theo giờ
    /// </summary>
    public class CheckInHourlyStatsDto
    {
        public int Hour { get; set; }
        public int CheckInCount { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê tenant
    /// </summary>
    public class TenantStatsDto
    {
        public int TotalEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int TotalGuests { get; set; }
        public int TotalCheckIns { get; set; }
        public int TotalUsers { get; set; }
        public double CheckInRate { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê thanh toán
    /// </summary>
    public class PaymentStatsDto
    {
        public int TotalPayments { get; set; }
        public int CompletedPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePaymentAmount { get; set; }
    }

    /// <summary>
    /// DTO cho doanh thu theo tháng
    /// </summary>
    public class MonthlyRevenueDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int PaymentCount { get; set; }
    }

    /// <summary>
    /// DTO cho dữ liệu biểu đồ
    /// </summary>
    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê người dùng
    /// </summary>
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<ChartDataPoint> UserGrowth { get; set; } = new();
        public Dictionary<string, int> UsersByRole { get; set; } = new();
    }

    /// <summary>
    /// DTO cho sự kiện hàng đầu
    /// </summary>
    public class TopEventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public int CheckInCount { get; set; }
        public double CheckInRate => GuestCount > 0 ? (double)CheckInCount / GuestCount * 100 : 0;
        public DateTime EventDate { get; set; }
    }

    /// <summary>
    /// DTO cho check-in gần đây
    /// </summary>
    public class RecentCheckInDto
    {
        public int CheckInId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
        public string CheckInType { get; set; } = string.Empty; // "in" hoặc "out"
    }

    /// <summary>
    /// DTO cho dữ liệu realtime
    /// </summary>
    public class RealTimeDataDto
    {
        public int ActiveCheckIns { get; set; }
        public int PendingCheckIns { get; set; }
        public List<RecentCheckInDto> RecentActivity { get; set; } = new();
        public DateTime LastUpdate { get; set; }
    }

    /// <summary>
    /// DTO cho metrics hiệu suất
    /// </summary>
    public class PerformanceMetricsDto
    {
        public double AverageResponseTime { get; set; }
        public int TotalRequests { get; set; }
        public int ErrorRate { get; set; }
        public int ActiveConnections { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
    }

    /// <summary>
    /// DTO cho analytics
    /// </summary>
    public class AnalyticsDto
    {
        public string Metric { get; set; } = string.Empty;
        public List<ChartDataPoint> Data { get; set; } = new();
        public Dictionary<string, object> Summary { get; set; } = new();
    }

    /// <summary>
    /// DTO cho export dữ liệu
    /// </summary>
    public class ExportResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho bulk operations
    /// </summary>
    public class BulkOperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ProcessedCount { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// DTO cho search result
    /// </summary>
    public class SearchResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public string Query { get; set; } = string.Empty;
        public TimeSpan SearchTime { get; set; }
    }

    /// <summary>
    /// DTO cho file upload
    /// </summary>
    public class FileUploadResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho notification
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "info", "success", "warning", "error"
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
    }

    /// <summary>
    /// DTO cho audit log
    /// </summary>
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho check-out
    /// </summary>
    public class CheckOutDto
    {
        [Required]
        public string QRCode { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? DeviceId { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cho bulk check-in
    /// </summary>
    public class BulkCheckInDto
    {
        [Required]
        public int EventId { get; set; }
        
        [Required]
        public List<string> QRCodes { get; set; } = new List<string>();
        
        [StringLength(200)]
        public string? DeviceId { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cho bulk gửi lời mời
    /// </summary>
    public class BulkSendInvitationsDto
    {
        [Required]
        public int EventId { get; set; }
        
        public List<int> GuestIds { get; set; } = new List<int>();
        
        public bool SendToAll { get; set; } = false;
        
        [StringLength(500)]
        public string? CustomMessage { get; set; }
    }

    /// <summary>
    /// DTO cho nâng cấp gói
    /// </summary>
    public class UpgradePlanDto
    {
        [Required]
        public ServicePlan NewPlan { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        public int DurationMonths { get; set; } = 1;
        
        public string? PromoCode { get; set; }
    }

    /// <summary>
    /// DTO cho hủy subscription
    /// </summary>
    public class CancelSubscriptionDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public bool CancelImmediately { get; set; } = false;
        
        public string? Feedback { get; set; }
    }

    /// <summary>
    /// DTO cho hoàn tiền
    /// </summary>
    public class RefundPaymentDto
    {
        [Required]
        public int PaymentId { get; set; }
        
        [Required]
        public decimal RefundAmount { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cho bulk update guests
    /// </summary>
    public class BulkUpdateGuestsDto
    {
        [Required]
        public List<int> GuestIds { get; set; } = new List<int>();
        
        public string? Group { get; set; }
        
        public string? TableNumber { get; set; }
        
        public bool? IsVIP { get; set; }
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cho xử lý thanh toán
    /// </summary>
    public class ProcessPaymentDto
    {
        [Required]
        public string PaymentReference { get; set; } = string.Empty;
        
        [Required]
        public PaymentStatus Status { get; set; }
        
        public string? TransactionId { get; set; }
        
        public string? Signature { get; set; }
        
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    public enum EventStatus { Upcoming, Ongoing, Ended }
    public enum GuestType { Regular, VIP }
    public enum InvitationStatus { Pending, Sent, Confirmed, Cancelled }
    public enum CheckInStatus { CheckedIn = 1, CheckedOut = 2 }
    public enum CheckInType { Manual = 1, QRCode = 2, Mobile = 3 }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string? PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public decimal? RefundedAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CreatePaymentRequest
    {
        public int EventId { get; set; }
        public int GuestId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class PaymentCallback
    {
        public string PaymentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class CheckInQRData
    {
        public int EventId { get; set; }
        public int GuestId { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
} 