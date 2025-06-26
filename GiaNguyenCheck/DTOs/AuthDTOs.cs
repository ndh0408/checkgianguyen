using System.ComponentModel.DataAnnotations;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.DTOs
{
    /// <summary>
    /// DTO cho đăng ký user trong tenant (cần mã mời)
    /// </summary>
    public class RegisterDto
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
        
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        /// <summary>
        /// Mã mời từ tenant admin (bắt buộc)
        /// </summary>
        [Required(ErrorMessage = "Mã mời là bắt buộc")]
        public string InvitationCode { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO cho đăng nhập
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; } = false;
    }
    
    /// <summary>
    /// DTO cho kết quả đăng nhập
    /// </summary>
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto? User { get; set; }
    }
    
    /// <summary>
    /// DTO cho làm mới token
    /// </summary>
    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO cho đổi mật khẩu
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Xác nhận mật khẩu mới là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO cho quên mật khẩu
    /// </summary>
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO cho đặt lại mật khẩu
    /// </summary>
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Xác nhận mật khẩu mới là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho xác nhận email
    /// </summary>
    public class ConfirmEmailDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho response đăng nhập
    /// </summary>
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public List<string> Roles { get; set; } = new();
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho response đăng ký
    /// </summary>
    public class RegisterResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO cho cập nhật thông tin profile
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ không được vượt quá 100 ký tự")]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// DTO cho đăng ký tenant mới (chỉ SystemAdmin mới được tạo)
    /// </summary>
    public class RegisterTenantDto
    {
        [Required(ErrorMessage = "Tên tổ chức là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tổ chức không được vượt quá 100 ký tự")]
        public string TenantName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? TenantDescription { get; set; }

        [Required(ErrorMessage = "Email tổ chức là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string TenantEmail { get; set; } = string.Empty;

        [StringLength(20)]
        public string? TenantPhone { get; set; }

        [StringLength(200)]
        public string? TenantAddress { get; set; }

        [Required(ErrorMessage = "Subdomain là bắt buộc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Subdomain phải từ 3-50 ký tự")]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Subdomain chỉ được chứa chữ thường, số và dấu gạch ngang")]
        public string Subdomain { get; set; } = string.Empty;

        // Thông tin admin của tenant
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(100)]
        public string AdminFirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(100)]
        public string AdminLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email admin là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string AdminEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string AdminPassword { get; set; } = string.Empty;

        /// <summary>
        /// Gói dịch vụ được cấp (mặc định Free)
        /// </summary>
        public int ServicePlan { get; set; } = 0; // Free

        /// <summary>
        /// Thời gian hết hạn gói (nếu không set thì mặc định 30 ngày cho Free)
        /// </summary>
        public DateTime? PlanExpiryDate { get; set; }

        /// <summary>
        /// Ghi chú từ admin
        /// </summary>
        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// DTO tạo mã mời cho user mới
    /// </summary>
    public class CreateInvitationDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int Role { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        /// <summary>
        /// Thời gian hết hạn mã mời (mặc định 7 ngày)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO thông tin mã mời
    /// </summary>
    public class InvitationDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Message { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    public class AuthDTOs
    {
        public class LoginViewModel
        {
            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }

            public string? TenantId { get; set; }
        }

        public class RegisterViewModel
        {
            [Required(ErrorMessage = "Họ và tên là bắt buộc")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = "";

            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = "";

            [Display(Name = "Số điện thoại")]
            public string? PhoneNumber { get; set; }

            // Tenant information
            [Required(ErrorMessage = "Tên công ty là bắt buộc")]
            [Display(Name = "Tên công ty")]
            public string CompanyName { get; set; } = "";

            [Required(ErrorMessage = "Subdomain là bắt buộc")]
            [Display(Name = "Subdomain")]
            public string Subdomain { get; set; } = "";

            [Display(Name = "Gói dịch vụ")]
            public string PlanId { get; set; } = "";
        }

        public class ForgotPasswordViewModel
        {
            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = "";
        }

        public class ResetPasswordViewModel
        {
            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu mới")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = "";

            public string Code { get; set; } = "";
        }

        public class ChangePasswordViewModel
        {
            [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu hiện tại")]
            public string CurrentPassword { get; set; } = "";

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            public string NewPassword { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu mới")]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = "";
        }

        public class UserProfileViewModel
        {
            [Required(ErrorMessage = "Họ và tên là bắt buộc")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = "";

            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = "";

            [Display(Name = "Số điện thoại")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Avatar")]
            public string? AvatarUrl { get; set; }

            [Display(Name = "Địa chỉ")]
            public string? Address { get; set; }

            [Display(Name = "Ngày sinh")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }
        }

        public class LoginResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public string? Token { get; set; }
            public User? User { get; set; }
            public string? RedirectUrl { get; set; }
        }

        public class RegisterResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public string? UserId { get; set; }
            public string? TenantId { get; set; }
        }
    }
} 