using System.ComponentModel.DataAnnotations;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Entity đại diện cho tổ chức/công ty trong hệ thống
    /// </summary>
    public class Tenant : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(200)]
        public string? Website { get; set; }
        
        [StringLength(200)]
        public string? LogoUrl { get; set; }
        
        /// <summary>
        /// Gói dịch vụ hiện tại
        /// </summary>
        public ServicePlan CurrentPlan { get; set; } = ServicePlan.Free;
        
        /// <summary>
        /// Ngày hết hạn gói dịch vụ
        /// </summary>
        public DateTime? PlanExpiryDate { get; set; }
        
        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Cài đặt tùy chỉnh (JSON)
        /// </summary>
        public string? CustomSettings { get; set; }
        
        /// <summary>
        /// Subdomain dành riêng cho tenant (tenant.myapp.com)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Subdomain { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
    
    /// <summary>
    /// Enum định nghĩa các gói dịch vụ
    /// </summary>
    public enum ServicePlan
    {
        Free = 0,
        Basic = 1,
        Pro = 2,
        Enterprise = 3
    }
} 