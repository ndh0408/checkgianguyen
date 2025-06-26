using System.ComponentModel.DataAnnotations;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Entity đại diện cho sự kiện
    /// </summary>
    public class Event : TenantEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
        
        /// <summary>
        /// Thời gian bắt đầu sự kiện
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Thời gian kết thúc sự kiện
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// Thời gian bắt đầu check-in
        /// </summary>
        public DateTime CheckInStartTime { get; set; }
        
        /// <summary>
        /// Thời gian kết thúc check-in
        /// </summary>
        public DateTime CheckInEndTime { get; set; }
        
        /// <summary>
        /// Ảnh sự kiện
        /// </summary>
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// Ảnh banner sự kiện
        /// </summary>
        [StringLength(300)]
        public string? BannerImageUrl { get; set; }
        
        /// <summary>
        /// Tên người tổ chức
        /// </summary>
        [StringLength(200)]
        public string? OrganizerName { get; set; }
        
        /// <summary>
        /// Số điện thoại người tổ chức
        /// </summary>
        [StringLength(20)]
        public string? OrganizerPhone { get; set; }
        
        /// <summary>
        /// Email người tổ chức
        /// </summary>
        [EmailAddress]
        [StringLength(200)]
        public string? OrganizerEmail { get; set; }
        
        /// <summary>
        /// Trạng thái sự kiện
        /// </summary>
        public EventStatus Status { get; set; } = EventStatus.Draft;
        
        /// <summary>
        /// Số lượng khách tối đa
        /// </summary>
        public int? MaxGuests { get; set; }
        
        /// <summary>
        /// Số lượng khách tối đa (legacy field for backward compatibility)
        /// </summary>
        public int MaxAttendees 
        { 
            get => MaxGuests ?? 0; 
            set => MaxGuests = value == 0 ? null : value; 
        }
        
        /// <summary>
        /// Sự kiện công khai
        /// </summary>
        public bool IsPublic { get; set; } = true;
        
        /// <summary>
        /// Cho phép check-in nhiều lần
        /// </summary>
        public bool AllowMultipleCheckIn { get; set; } = false;
        
        /// <summary>
        /// Cài đặt sự kiện (JSON)
        /// </summary>
        public string? Settings { get; set; }
        
        /// <summary>
        /// ID người tạo sự kiện
        /// </summary>
        public int CreatedByUserId { get; set; }
        
        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ICollection<Guest> Guests { get; set; } = new List<Guest>();
        public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
        
        /// <summary>
        /// Tổng số khách đã đăng ký
        /// </summary>
        public int TotalGuests => Guests.Count(g => !g.IsDeleted);
        
        /// <summary>
        /// Tổng số khách đã check-in
        /// </summary>
        public int TotalCheckedIn => CheckIns.Count(c => c.CheckInTime != null && c.Status == CheckInStatus.CheckedIn);
    }
    
    /// <summary>
    /// Enum trạng thái sự kiện
    /// </summary>
    public enum EventStatus
    {
        /// <summary>
        /// Bản nháp
        /// </summary>
        Draft = 0,
        
        /// <summary>
        /// Đã xuất bản
        /// </summary>
        Published = 1,
        
        /// <summary>
        /// Đang diễn ra
        /// </summary>
        Ongoing = 2,
        
        /// <summary>
        /// Đã kết thúc
        /// </summary>
        Completed = 3,
        
        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 4
    }
    
    /// <summary>
    /// Entity liên kết nhân viên với sự kiện
    /// </summary>
    public class EventStaff : BaseEntity
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        
        /// <summary>
        /// Vai trò trong sự kiện
        /// </summary>
        public EventStaffRole Role { get; set; } = EventStaffRole.CheckInStaff;
        
        // Navigation properties
        public virtual Event Event { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
    
    /// <summary>
    /// Enum vai trò nhân viên trong sự kiện
    /// </summary>
    public enum EventStaffRole
    {
        /// <summary>
        /// Quản lý sự kiện
        /// </summary>
        Manager = 0,
        
        /// <summary>
        /// Nhân viên check-in
        /// </summary>
        CheckInStaff = 1,
        
        /// <summary>
        /// Nhân viên hỗ trợ
        /// </summary>
        Support = 2
    }
} 