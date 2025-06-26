using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.DTOs.CommonDTOs;

namespace GiaNguyenCheck.Interfaces
{
    /// <summary>
    /// Interface cho Authentication Service
    /// </summary>
    public interface IAuthService
    {
        Task<ApiResponse<LoginResultDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<LoginResultDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<ApiResponse<bool>> LogoutAsync(int userId);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<ApiResponse<UserDto>> GetCurrentUserAsync();
    }
    
    /// <summary>
    /// Interface cho Tenant Service
    /// </summary>
    public interface ITenantService
    {
        Task<ApiResponse<TenantDto>> CreateTenantAsync(CreateTenantDto createTenantDto);
        Task<ApiResponse<TenantDto>> GetTenantAsync(int tenantId);
        Task<ApiResponse<TenantDto>> GetTenantBySubdomainAsync(string subdomain);
        Task<ApiResponse<PagedResult<TenantDto>>> GetTenantsAsync(int page = 1, int pageSize = 10, string searchTerm = "");
        Task<ApiResponse<TenantDto>> UpdateTenantAsync(int tenantId, UpdateTenantDto updateTenantDto);
        Task<ApiResponse<bool>> DeleteTenantAsync(int tenantId);
        Task<ApiResponse<bool>> UpdateTenantPlanAsync(int tenantId, string newPlan);
        Task<ApiResponse<TenantStatsDto>> GetTenantStatsAsync(int tenantId);
        Task<ApiResponse<List<TenantDto>>> GetExpiringTenantsAsync(int daysThreshold = 7);
        Task<ApiResponse<bool>> ActivateTenantAsync(int tenantId);
        Task<ApiResponse<bool>> DeactivateTenantAsync(int tenantId);
    }

    /// <summary>
    /// Interface cho TenantManagementService (chỉ SystemAdmin)
    /// </summary>
    public interface ITenantManagementService
    {
        Task<ApiResponse<TenantDto>> CreateTenantAsync(RegisterTenantDto dto);
        Task<ApiResponse<PagedResult<TenantDto>>> GetTenantsAsync(int page = 1, int pageSize = 20, string? search = null);
        Task<ApiResponse<InvitationDto>> CreateInvitationAsync(CreateInvitationDto dto, int tenantId, int createdByUserId);
        Task<ApiResponse<InvitationDto>> ValidateInvitationAsync(string code);
        Task<bool> MarkInvitationAsUsedAsync(string code, int usedByUserId);
    }

    /// <summary>
    /// Interface cho Dashboard Service
    /// </summary>
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync();
        Task<ApiResponse<EventStatsDto>> GetEventsStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<CheckInStatsDto>> GetCheckInStatsAsync(int eventId = 0, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<PaymentStatsDto>> GetRevenueStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<UserStatsDto>> GetUserStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<List<TopEventDto>>> GetTopEventsAsync(int limit = 10);
        Task<ApiResponse<List<RecentCheckInDto>>> GetRecentCheckInsAsync(int limit = 20);
        Task<ApiResponse<byte[]>> ExportReportAsync(string reportType, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<RealTimeDataDto>> GetRealTimeDataAsync();
        Task<ApiResponse<AnalyticsDto>> GetAnalyticsAsync(string metric, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<PerformanceMetricsDto>> GetPerformanceMetricsAsync();
        Task<CheckInStatsDto> GetCheckInStatsAsync(string period);
        Task<byte[]> GenerateReportAsync();
        Task<SystemMetrics> GetSystemMetricsAsync();
        Task<SystemHealth> GetSystemHealthAsync();
        Task<PlatformMetrics> GetPlatformMetricsAsync();
        Task<SystemPerformance> GetSystemPerformanceAsync();
        Task<List<QuickAction>> GetQuickActionsAsync(string? userId);
        Task<List<ExportOption>> GetExportOptionsAsync();
        Task<byte[]> GenerateReportAsync(ExportReportViewModel model, string? userId);
    }
    
    /// <summary>
    /// Interface cho Event Service
    /// </summary>
    public interface IEventService
    {
        Task<ApiResponse<EventDto>> CreateEventAsync(CreateEventDto createEventDto);
        Task<ApiResponse<EventDto>> GetEventAsync(int eventId);
        Task<ApiResponse<PagedResult<EventDto>>> GetEventsAsync(int page = 1, int pageSize = 10, string searchTerm = "", string status = "");
        Task<ApiResponse<EventDto>> UpdateEventAsync(int eventId, UpdateEventDto updateEventDto);
        Task<ApiResponse<bool>> DeleteEventAsync(int eventId);
        Task<ApiResponse<List<EventDto>>> GetUpcomingEventsAsync(int days = 30);
        Task<ApiResponse<EventStatsDto>> GetEventStatsAsync(int eventId);
        Task<ApiResponse<byte[]>> GenerateEventQRCodeAsync(int eventId);
        Task<ApiResponse<bool>> PublishEventAsync(int eventId);
        Task<ApiResponse<bool>> UnpublishEventAsync(int eventId);
        Task<List<Event>> GetEventsByTenantAsync(int tenantId);
        Task<Event?> GetEventByIdAsync(int id);
        Task<bool> UpdateEventAsync(Event eventModel);
        Task<bool> CreateEventAsync(Event eventModel);
        Task<int> GetTotalEventsAsync();
        Task<int> GetTotalEventsByTenantAsync(string? tenantId);
        Task<int> GetTotalCheckinsAsync();
        Task<int> GetTotalGuestsByTenantAsync(string? tenantId);
        Task<decimal> GetSuccessRateByTenantAsync(string? tenantId);
        Task<List<Event>> GetRecentEventsByTenantAsync(string? tenantId, int count);
        Task<List<Event>> GetEventsByManagerAsync(string? userId);
        Task<List<Event>> GetUpcomingEventsAsync(string? userId, int count);
        Task<EventStatistics> GetEventStatisticsAsync(string? userId);
        Task<List<Event>> GetEventsByTenantAsync(string? tenantId);
        Task<EventCalendar> GetEventCalendarAsync(string? tenantId);
        Task<ResourceAllocation> GetResourceAllocationAsync(string? tenantId);
        Task<List<EventTemplate>> GetEventTemplatesAsync();
        Task<bool> CreateEventAsync(CreateEventViewModel model, string? userId);
        Task<bool> AssignStaffAsync(AssignStaffViewModel model);
        Task<EventAnalytics> GetEventAnalyticsAsync(string? userId);
        Task<EventTrends> GetEventTrendsAsync(string? tenantId);
        Task<List<Event>> GetTodaysEventsAsync(string? userId);
        Task<List<EventDetail>> GetEventDetailsAsync(string? userId);
    }
    
    /// <summary>
    /// Interface cho Guest Service
    /// </summary>
    public interface IGuestService
    {
        Task<ApiResponse<GuestDto>> CreateGuestAsync(CreateGuestDto createGuestDto);
        Task<ApiResponse<GuestDto>> GetGuestAsync(int guestId);
        Task<ApiResponse<PagedResult<GuestDto>>> GetGuestsAsync(int eventId, int page = 1, int pageSize = 10, string searchTerm = "");
        Task<ApiResponse<GuestDto>> UpdateGuestAsync(int guestId, UpdateGuestDto updateGuestDto);
        Task<ApiResponse<bool>> DeleteGuestAsync(int guestId);
        Task<ApiResponse<ImportResultDto>> ImportGuestsAsync(int eventId, List<CreateGuestDto> guests);
        Task<ApiResponse<byte[]>> GenerateGuestQRCodeAsync(int guestId);
        Task<ApiResponse<bool>> SendInvitationEmailAsync(int guestId);
        Task<ApiResponse<bool>> SendBulkInvitationsAsync(int eventId);
        Task<int> GetTotalGuestsAsync();
        Task<GuestStatistics> GetGuestStatisticsAsync(string? userId);
        Task<List<GuestList>> GetGuestListsAsync(string? userId);
        Task<List<ImportHistory>> GetImportHistoryAsync(string? userId);
        Task<bool> ImportGuestsAsync(IFormFile file, string eventId, string? userId);
        Task<List<Guest>> SearchGuestByNameAsync(string name, string? userId);
        Task<List<Guest>> GetEventGuestsAsync(string eventId, string? userId);
        Task<GuestAnalytics> GetGuestAnalyticsAsync(string? userId);
        Task<List<GuestTemplate>> GetGuestTemplatesAsync();
        Task<List<SearchHistory>> GetSearchHistoryAsync(string? userId);
    }
    
    /// <summary>
    /// Interface cho Check-In Service
    /// </summary>
    public interface ICheckInService
    {
        Task<ApiResponse<CheckInDto>> CheckInGuestAsync(string qrCodeData, int checkedInByUserId);
        Task<ApiResponse<CheckInDto>> CheckOutGuestAsync(int guestId, int eventId, int checkedOutByUserId);
        Task<ApiResponse<CheckInDto>> GetCheckInAsync(int checkInId);
        Task<ApiResponse<PagedResult<CheckInDto>>> GetCheckInsAsync(int eventId, int page = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<CheckInStatsDto>> GetCheckInStatsAsync(int eventId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<List<CheckInTrendDto>>> GetCheckInTrendsAsync(int eventId, int days = 7);
        Task<ApiResponse<List<CheckInHourlyStatsDto>>> GetHourlyCheckInStatsAsync(int eventId, DateTime date);
        Task<ApiResponse<bool>> SyncCheckInsAsync(int eventId);
        Task<ApiResponse<bool>> ExportCheckInDataAsync(int eventId, string format = "csv");
        Task<List<CheckIn>> GetRecentCheckinsAsync(string? userId, int count);
        Task<LiveStatistics> GetLiveStatisticsAsync(string? userId);
        Task<List<CheckIn>> GetCheckInHistoryAsync(string? userId);
        Task<ScannerSettings> GetScannerSettingsAsync();
        Task<CheckInResult> ProcessCheckInAsync(ProcessCheckInViewModel model, string? userId);
        Task<CheckInResult> ManualCheckInAsync(ManualCheckInViewModel model, string? userId);
        Task<bool> UpdateCheckInStatusAsync(UpdateCheckInStatusViewModel model, string? userId);
        Task<CheckInStats> GetCheckInStatsAsync(string eventId, string? userId);
        Task<CheckInReports> GetCheckInReportsAsync(string? userId);
        Task<TodayStats> GetTodayStatsAsync(string? userId);
        Task<WeeklyStats> GetWeeklyStatsAsync(string? userId);
        Task<List<RecentActivity>> GetRecentActivityAsync(string? userId);
    }
    
    /// <summary>
    /// Interface cho Payment Service
    /// </summary>
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, string tenantId);
        Task<PaymentResponse> ProcessPaymentCallbackAsync(string paymentMethod, string callbackData);
        Task<PaymentResponse> GetPaymentStatusAsync(string paymentId, string tenantId);
        Task<List<PaymentResponse>> GetPaymentsByEventAsync(int eventId, string tenantId);
        Task<PaymentResponse> RefundPaymentAsync(string paymentId, decimal amount, string tenantId);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetMonthlyRevenueAsync();
        Task<decimal> GetChurnRateAsync();
        Task<List<Payment>> GetPaymentHistoryAsync();
        Task<List<Payment>> GetPaymentHistoryByTenantAsync(string? tenantId);
        Task<RevenueByPlan> GetRevenueByPlanAsync();
        Task<decimal> GetMonthlyRecurringRevenueAsync();
        Task<RevenueChartData> GetRevenueChartDataAsync();
    }
    
    /// <summary>
    /// Interface cho QR Code Service
    /// </summary>
    public interface IQRCodeService
    {
        Task<byte[]> GenerateQRCodeAsync(string data);
        Task<byte[]> GenerateEncryptedQRCodeAsync(string data, string encryptionKey);
        Task<string> DecryptQRCodeDataAsync(string encryptedData, string encryptionKey);
        Task<CheckInQRData> GenerateCheckInQRDataAsync(int eventId, int guestId, string tenantId);
        Task<(bool isValid, CheckInQRData? data)> ValidateCheckInQRCodeAsync(string qrCodeData, string tenantId);
        Task<string> GenerateQRCodeAsync(string data);
        Task<QRCodeData> GetQRCodeDataAsync();
        Task<QRCodeSettings> GetQRCodeSettingsAsync();
    }
    
    /// <summary>
    /// Interface cho Email Service
    /// </summary>
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendInvitationEmailAsync(Invitation invitation, string tenantName);
        Task<bool> SendWelcomeEmailAsync(User user, string tenantName);
        Task<bool> SendPasswordResetEmailAsync(User user, string resetToken);
        Task<bool> SendEventReminderEmailAsync(Guest guest, Event eventEntity);
        Task<bool> SendPaymentConfirmationEmailAsync(EventPayment payment, Guest guest, Event eventEntity);
        Task<bool> SendCheckInConfirmationEmailAsync(CheckIn checkIn, Guest guest, Event eventEntity);
        Task<bool> SendEventInvitationAsync(string to, string guestName, string eventName, string eventDetails, string qrCodeUrl, string eventUrl);
        Task<bool> SendRegistrationConfirmationAsync(string to, string userName, string confirmationUrl);
        Task<bool> SendPasswordResetAsync(string to, string userName, string resetUrl);
        Task<bool> SendCheckInNotificationAsync(string to, string guestName, string eventName, DateTime checkInTime);
        Task<List<bool>> SendBulkEmailAsync(List<string> recipients, string subject, string bodyTemplate, Dictionary<string, string>? placeholders = null);
        Task<bool> SendInvitationsAsync(SendInvitationViewModel model, string? userId);
        Task<List<EmailCampaign>> GetEmailCampaignsAsync(string? userId);
        Task<List<SMSTemplate>> GetSMSTemplatesAsync();
        Task<List<SentEmail>> GetSentEmailsAsync(string? userId);
        Task<List<EmailTemplate>> GetEmailTemplatesAsync();
        Task<InvitationStatus> GetInvitationStatusAsync(string? userId);
    }
    
    /// <summary>
    /// Interface cho File Upload Service
    /// </summary>
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<byte[]> DownloadFileAsync(string fileUrl);
        bool IsValidImageFile(string fileName);
        bool IsValidDocumentFile(string fileName);
        string GenerateUniqueFileName(string originalFileName);
    }
    
    /// <summary>
    /// Interface cho Export Service
    /// </summary>
    public interface IExportService
    {
        Task<ApiResponse<byte[]>> ExportGuestsToExcelAsync(int eventId);
        Task<ApiResponse<byte[]>> ExportCheckInsToExcelAsync(int eventId);
        Task<ApiResponse<byte[]>> GenerateEventReportPdfAsync(int eventId);
        Task<ApiResponse<byte[]>> GenerateInvitationPdfAsync(Guest guest, byte[] qrCodeImage);
    }
    
    /// <summary>
    /// Interface cho Import Service
    /// </summary>
    public interface IImportService
    {
        Task<ApiResponse<ImportResultDto>> ImportGuestsFromExcelAsync(int eventId, Stream excelStream);
        Task<ApiResponse<ImportResultDto>> ImportGuestsFromCsvAsync(int eventId, Stream csvStream);
        Task<ApiResponse<List<CreateUpdateGuestDto>>> ParseExcelFileAsync(Stream excelStream);
        Task<ApiResponse<List<CreateUpdateGuestDto>>> ParseCsvFileAsync(Stream csvStream);
    }
    
    /// <summary>
    /// Interface cho Notification Service (SignalR)
    /// </summary>
    public interface INotificationService
    {
        Task NotifyCheckInAsync(int eventId, CheckInDto checkIn);
        Task NotifyEventStatsUpdateAsync(int eventId, EventStatsDto stats);
        Task NotifyPaymentSuccessAsync(int tenantId, PaymentDto payment);
        Task SendMessageToTenantAsync(int tenantId, string message);
        Task SendMessageToEventStaffAsync(int eventId, string message);
        Task SendMessageToUserAsync(int userId, string message);
    }
    
    /// <summary>
    /// Interface cho Service Limit Checker
    /// </summary>
    public interface IServiceLimitService
    {
        Task<ApiResponse<bool>> CanCreateEventAsync(int tenantId);
        Task<ApiResponse<bool>> CanAddGuestAsync(int eventId);
        Task<ApiResponse<bool>> HasReachedConcurrentCheckInLimitAsync(int tenantId);
        Task<ApiResponse<ServiceLimitStatusDto>> GetServiceLimitsAsync(int tenantId);
        Task<ApiResponse<ServiceLimitStatusDto>> GetServiceUsageAsync(int tenantId);
    }

    public interface IUserService
    {
        Task<User?> GetCurrentUserAsync();
        Task<int> GetTotalUsersAsync();
        Task<int> GetTotalEmployeesByTenantAsync(string? tenantId);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetEmployeesByTenantAsync(string? tenantId);
        Task<RolesAssignment> GetRolesAssignmentAsync(string? tenantId);
        Task<List<Permission>> GetPermissionsAsync();
        Task<UserGrowth> GetUserGrowthAsync();
        Task<List<User>> GetAvailableStaffAsync();
        Task<List<User>> GetAssignedStaffAsync(string? userId);
        Task<EmployeeActivity> GetEmployeeActivityAsync(string? tenantId);
        Task<EmployeePerformance> GetEmployeePerformanceAsync(string? tenantId);
        Task<bool> CreateEmployeeAsync(CreateEmployeeViewModel model, string? tenantId);
        Task<bool> UpdateEmployeeRoleAsync(string userId, string role);
        Task<bool> UpdateUserRoleAsync(string userId, string role);
        Task<PerformanceMetrics> GetPerformanceMetricsAsync(string? userId);
    }
}

// DTOs
namespace GiaNguyenCheck.DTOs.CommonDTOs
{
    public class CreateEmployeeViewModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Role { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RolesAssignment
    {
        public int EventManagerCount { get; set; }
        public int StaffCount { get; set; }
        public int ViewerCount { get; set; }
    }

    public class Permission
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }

    public class UserGrowth
    {
        public List<string> Months { get; set; } = new();
        public List<int> NewUsers { get; set; } = new();
        public List<int> ActiveUsers { get; set; } = new();
    }

    public class EmployeeActivity
    {
        public List<string> Dates { get; set; } = new();
        public List<int> ActiveEmployees { get; set; } = new();
    }

    public class EmployeePerformance
    {
        public List<User> TopPerformers { get; set; } = new();
        public List<Event> RecentCheckins { get; set; } = new();
    }

    public class PerformanceMetrics
    {
        public int TotalCheckins { get; set; }
        public decimal AverageCheckinTime { get; set; }
        public decimal AccuracyRate { get; set; }
        public int CustomerSatisfaction { get; set; }
    }

    public class RevenueChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
    }

    public class QRCodeData
    {
        public string EventId { get; set; } = "";
        public string EventName { get; set; } = "";
        public string GuestId { get; set; } = "";
        public string GuestName { get; set; } = "";
        public string QRCode { get; set; } = "";
    }

    public class QRCodeSettings
    {
        public string Size { get; set; } = "medium";
        public string Color { get; set; } = "#000000";
        public bool IncludeLogo { get; set; } = true;
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

    public class UpdateCheckInStatusViewModel
    {
        public string CheckInId { get; set; } = "";
        public string Status { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class CheckInResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public Guest? Guest { get; set; }
        public DateTime? CheckInTime { get; set; }
    }

    public class LiveStatistics
    {
        public int TotalCheckins { get; set; }
        public int TodayCheckins { get; set; }
        public int PendingCheckins { get; set; }
        public decimal CheckinRate { get; set; }
    }

    public class ScannerSettings
    {
        public bool EnableSound { get; set; } = true;
        public bool EnableVibration { get; set; } = true;
        public string CameraResolution { get; set; } = "HD";
        public bool AutoFocus { get; set; } = true;
    }

    public class CheckInStats
    {
        public int TotalGuests { get; set; }
        public int CheckedInGuests { get; set; }
        public decimal CheckInRate { get; set; }
        public List<string> TimeSlots { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
    }

    public class CheckInReports
    {
        public List<CheckIn> RecentCheckins { get; set; } = new();
        public List<string> TimeSlots { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
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

    public class RecentActivity
    {
        public string Id { get; set; } = "";
        public string Action { get; set; } = "";
        public string GuestName { get; set; } = "";
        public string EventName { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "";
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

    public class EventAnalytics
    {
        public List<string> EventNames { get; set; } = new();
        public List<int> GuestCounts { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
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

    public class GuestTemplate
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class SearchHistory
    {
        public string Id { get; set; } = "";
        public string SearchTerm { get; set; } = "";
        public DateTime SearchDate { get; set; }
        public int ResultsCount { get; set; }
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

    public class ExportOption
    {
        public string Name { get; set; } = "";
        public string Format { get; set; } = "";
        public string Description { get; set; } = "";
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

    public class SystemMetrics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal ConversionRate { get; set; }
    }

    public class SystemHealth
    {
        public string Status { get; set; } = "Healthy";
        public decimal CpuUsage { get; set; }
        public decimal MemoryUsage { get; set; }
        public decimal DiskUsage { get; set; }
    }

    public class PlatformMetrics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal ConversionRate { get; set; }
        public int TotalEvents { get; set; }
    }

    public class SystemPerformance
    {
        public decimal ResponseTime { get; set; }
        public decimal Uptime { get; set; }
        public int ErrorRate { get; set; }
    }
} 