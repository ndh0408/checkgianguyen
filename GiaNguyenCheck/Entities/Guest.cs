using System.ComponentModel.DataAnnotations;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Entity đại diện cho khách mời tham gia sự kiện
    /// </summary>
    public class Guest : TenantEntity
    {
        /// <summary>
        /// ID sự kiện mà khách này thuộc về
        /// </summary>
        public int EventId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Company { get; set; }
        
        [StringLength(100)]
        public string? Position { get; set; }
        
        /// <summary>
        /// Nhóm khách (VIP, Thường, etc.)
        /// </summary>
        [StringLength(50)]
        public string? Group { get; set; }
        
        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        [StringLength(500)]
        public string? AvatarUrl { get; set; }
        
        /// <summary>
        /// Loại khách (VIP, thường)
        /// </summary>
        public GuestType Type { get; set; } = GuestType.Regular;
        
        /// <summary>
        /// Khách VIP
        /// </summary>
        public bool IsVIP { get; set; } = false;
        
        /// <summary>
        /// Số bàn (nếu có)
        /// </summary>
        [StringLength(50)]
        public string? TableNumber { get; set; }
        
        /// <summary>
        /// Ghi chú đặc biệt
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
        
        /// <summary>
        /// Mã QR được tạo cho khách này
        /// </summary>
        [Required]
        [StringLength(500)]
        public string QRCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Hash của QR code để bảo mật
        /// </summary>
        [Required]
        [StringLength(200)]
        public string QRCodeHash { get; set; } = string.Empty;
        
        /// <summary>
        /// Thời gian gửi vé mời
        /// </summary>
        public DateTime? InvitationSentAt { get; set; }
        
        /// <summary>
        /// Trạng thái vé mời
        /// </summary>
        public InvitationStatus InvitationStatus { get; set; } = InvitationStatus.NotSent;
        
        /// <summary>
        /// Số lần gửi lại vé mời
        /// </summary>
        public int ResendCount { get; set; } = 0;
        
        /// <summary>
        /// Đã xác nhận tham gia
        /// </summary>
        public bool IsConfirmed { get; set; } = false;
        
        /// <summary>
        /// Thời gian xác nhận
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }
        
        // Navigation properties
        public virtual Event Event { get; set; } = null!;
        public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
        
        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
        
        /// <summary>
        /// Đã check-in chưa
        /// </summary>
        public bool IsCheckedIn => CheckIns.Any(c => c.CheckInTime != null && c.Status == CheckInStatus.CheckedIn);
        
        /// <summary>
        /// Thời gian check-in đầu tiên
        /// </summary>
        public DateTime? FirstCheckInTime => CheckIns.Where(c => c.CheckInTime != null).OrderBy(c => c.CheckInTime).FirstOrDefault()?.CheckInTime;
    }
    
    /// <summary>
    /// Enum loại khách
    /// </summary>
    public enum GuestType
    {
        /// <summary>
        /// Khách thường
        /// </summary>
        Regular = 0,
        
        /// <summary>
        /// Khách VIP
        /// </summary>
        VIP = 1,
        
        /// <summary>
        /// Khách đặc biệt
        /// </summary>
        Special = 2,
        
        /// <summary>
        /// Báo chí/truyền thông
        /// </summary>
        Media = 3,
        
        /// <summary>
        /// Nhà tài trợ
        /// </summary>
        Sponsor = 4
    }
    
    /// <summary>
    /// Enum trạng thái vé mời
    /// </summary>
    public enum InvitationStatus
    {
        /// <summary>
        /// Chưa gửi
        /// </summary>
        NotSent = 0,
        
        /// <summary>
        /// Đang gửi
        /// </summary>
        Sending = 1,
        
        /// <summary>
        /// Đã gửi thành công
        /// </summary>
        Sent = 2,
        
        /// <summary>
        /// Gửi thất bại
        /// </summary>
        Failed = 3,
        
        /// <summary>
        /// Đã xem
        /// </summary>
        Viewed = 4
    }
} 