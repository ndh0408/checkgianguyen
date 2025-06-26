namespace GiaNguyenCheck.Constants
{
    /// <summary>
    /// Các hằng số của ứng dụng
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Các claim types
        /// </summary>
        public static class Claims
        {
            public const string TenantId = "tenant_id";
            public const string Role = "role";
            public const string UserId = "user_id";
            public const string Email = "email";
            public const string FullName = "full_name";
        }
        
        /// <summary>
        /// Các role trong hệ thống
        /// </summary>
        public static class Roles
        {
            public const string SystemAdmin = "SystemAdmin";
            public const string TenantAdmin = "TenantAdmin";
            public const string EventManager = "EventManager";
            public const string Staff = "Staff";
            public const string Viewer = "Viewer";
        }
        
        /// <summary>
        /// Các policy authorization
        /// </summary>
        public static class Policies
        {
            public const string RequireSystemAdmin = "RequireSystemAdmin";
            public const string RequireTenantAdmin = "RequireTenantAdmin";
            public const string RequireEventManager = "RequireEventManager";
            public const string RequireStaff = "RequireStaff";
            public const string RequireViewer = "RequireViewer";
        }
        
        /// <summary>
        /// Giới hạn gói dịch vụ
        /// </summary>
        public static class ServiceLimits
        {
            public static class Free
            {
                public const int MaxEvents = 1;
                public const int MaxGuests = 50;
                public const int MaxConcurrentCheckIns = 1;
                public const bool HasWatermark = true;
            }
            
            public static class Basic
            {
                public const int MaxEvents = 5;
                public const int MaxGuests = 1000;
                public const int MaxConcurrentCheckIns = 3;
                public const bool HasWatermark = false;
            }
            
            public static class Pro
            {
                public const int MaxEvents = 20;
                public const int MaxGuests = 5000;
                public const int MaxConcurrentCheckIns = 10;
                public const bool HasWatermark = false;
                public const bool CanCustomizeTheme = true;
            }
            
            public static class Enterprise
            {
                public const int MaxEvents = int.MaxValue;
                public const int MaxGuests = int.MaxValue;
                public const int MaxConcurrentCheckIns = int.MaxValue;
                public const bool HasWatermark = false;
                public const bool CanCustomizeTheme = true;
                public const bool HasCustomDomain = true;
            }
        }
        
        /// <summary>
        /// Giá gói dịch vụ (VND)
        /// </summary>
        public static class ServicePrices
        {
            public const decimal Free = 0;
            public const decimal Basic = 299000;
            public const decimal Pro = 799000;
            public const decimal Enterprise = 0; // Liên hệ
        }
        
        /// <summary>
        /// Cache keys
        /// </summary>
        public static class CacheKeys
        {
            public const string TenantPrefix = "tenant:";
            public const string UserPrefix = "user:";
            public const string EventPrefix = "event:";
            public const string GuestPrefix = "guest:";
            public const string CheckInPrefix = "checkin:";
        }
        
        /// <summary>
        /// Thiết lập mặc định
        /// </summary>
        public static class DefaultSettings
        {
            public const int PageSize = 20;
            public const int MaxPageSize = 100;
            public const int TokenExpiryMinutes = 60;
            public const int RefreshTokenExpiryDays = 30;
            public const int QRCodeSize = 256;
            public const string DefaultTimeZone = "SE Asia Standard Time";
            public const string DefaultCurrency = "VND";
            public const string DefaultLanguage = "vi-VN";
        }
        
        /// <summary>
        /// File upload settings
        /// </summary>
        public static class FileUpload
        {
            public const int MaxFileSizeInMB = 10;
            public const long MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
            public static readonly string[] AllowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            public static readonly string[] AllowedDocumentTypes = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv" };
        }
        
        /// <summary>
        /// Email templates
        /// </summary>
        public static class EmailTemplates
        {
            public const string WelcomeEmail = "welcome";
            public const string InvitationEmail = "invitation";
            public const string PasswordResetEmail = "password-reset";
            public const string PaymentSuccessEmail = "payment-success";
            public const string EventReminderEmail = "event-reminder";
        }
        
        /// <summary>
        /// SignalR Hub names
        /// </summary>
        public static class Hubs
        {
            public const string CheckInHub = "/checkin-hub";
            public const string DashboardHub = "/dashboard-hub";
        }
        
        /// <summary>
        /// Background job types
        /// </summary>
        public static class JobTypes
        {
            public const string SendInvitations = "send-invitations";
            public const string ProcessPayments = "process-payments";
            public const string GenerateReports = "generate-reports";
            public const string CleanupExpiredTokens = "cleanup-expired-tokens";
            public const string SyncOfflineCheckIns = "sync-offline-checkins";
        }

        public static class ServicePlans
        {
            public const string Free = "Free";
            public const string Basic = "Basic";
            public const string Pro = "Pro";
            public const string Enterprise = "Enterprise";
            public static readonly string[] AllPlans = { Free, Basic, Pro, Enterprise };
        }

        public enum ServicePlan { Free, Basic, Pro, Enterprise }
        public enum PaymentStatus { Pending, Success, Failed, Cancelled }
        public enum PaymentMethod { Stripe, Momo, PayPal, VNPAY }
    }
} 