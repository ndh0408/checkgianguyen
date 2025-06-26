using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Entity người dùng mở rộng từ IdentityUser
    /// </summary>
    public class User : IdentityUser<int>
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? AvatarUrl { get; set; }
        
        /// <summary>
        /// ID của tenant mà user thuộc về
        /// </summary>
        public int TenantId { get; set; }
        
        /// <summary>
        /// Vai trò trong tổ chức
        /// </summary>
        public UserRole Role { get; set; } = UserRole.Staff;
        
        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Lần đăng nhập cuối
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
        
        /// <summary>
        /// Ngày tạo tài khoản
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Ngày cập nhật cuối
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        // Removed CheckIn navigation to avoid circular dependency
        // public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
        
        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
    }
    
    /// <summary>
    /// Enum định nghĩa vai trò người dùng
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Quản trị viên hệ thống
        /// </summary>
        SystemAdmin = 0,
        
        /// <summary>
        /// Chủ tổ chức
        /// </summary>
        TenantAdmin = 1,
        
        /// <summary>
        /// Quản lý sự kiện
        /// </summary>
        EventManager = 2,
        
        /// <summary>
        /// Nhân viên check-in
        /// </summary>
        Staff = 3,
        
        /// <summary>
        /// Người xem báo cáo
        /// </summary>
        Viewer = 4
    }
} 