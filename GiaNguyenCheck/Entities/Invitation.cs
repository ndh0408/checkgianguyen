using System.ComponentModel.DataAnnotations;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Entity quản lý mã mời cho user mới
    /// </summary>
    public class Invitation : TenantEntity
    {
        /// <summary>
        /// Email được mời
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mã mời duy nhất
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò được cấp
        /// </summary>
        public UserRole Role { get; set; } = UserRole.Staff;

        /// <summary>
        /// Thông điệp từ người mời
        /// </summary>
        [StringLength(500)]
        public string? Message { get; set; }

        /// <summary>
        /// Thời gian hết hạn
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Đã được sử dụng chưa
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Thời gian sử dụng
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// ID người tạo mã mời
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// ID user được tạo từ mã mời này (nếu đã sử dụng)
        /// </summary>
        public int? UsedByUserId { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual User? UsedByUser { get; set; }

        /// <summary>
        /// Kiểm tra mã mời có hợp lệ không
        /// </summary>
        public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
    }
} 