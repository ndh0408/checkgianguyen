using System.ComponentModel.DataAnnotations;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Entity đại diện cho thông tin check-in của khách
    /// </summary>
    public class CheckIn : TenantEntity
    {
        /// <summary>
        /// ID sự kiện
        /// </summary>
        public int EventId { get; set; }
        
        /// <summary>
        /// ID khách mời
        /// </summary>
        public int GuestId { get; set; }
        
        /// <summary>
        /// ID nhân viên thực hiện check-in
        /// </summary>
        public int? CheckedInByUserId { get; set; }
        
        /// <summary>
        /// Thời gian check-in
        /// </summary>
        public DateTime? CheckInTime { get; set; }
        
        /// <summary>
        /// Thời gian check-out
        /// </summary>
        public DateTime? CheckOutTime { get; set; }
        
        /// <summary>
        /// ID nhân viên thực hiện check-out
        /// </summary>
        public int? CheckedOutByUserId { get; set; }
        
        /// <summary>
        /// ID thiết bị check-in
        /// </summary>
        [StringLength(200)]
        public string? DeviceId { get; set; }
        
        /// <summary>
        /// Thiết bị thực hiện check-in
        /// </summary>
        [StringLength(200)]
        public string? DeviceInfo { get; set; }
        
        /// <summary>
        /// Địa chỉ IP
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }
        
        /// <summary>
        /// Vị trí GPS (nếu có)
        /// </summary>
        [StringLength(50)]
        public string? Location { get; set; }
        
        /// <summary>
        /// Ghi chú khi check-in
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
        
        /// <summary>
        /// Trạng thái check-in
        /// </summary>
        public CheckInStatus Status { get; set; } = CheckInStatus.CheckedIn;
        
        /// <summary>
        /// Loại check-in (thủ công, QR, tự động)
        /// </summary>
        public CheckInType Type { get; set; } = CheckInType.QRCode;
        
        /// <summary>
        /// Đã đồng bộ lên server chưa (cho offline mode)
        /// </summary>
        public bool IsSynced { get; set; } = true;
        
        /// <summary>
        /// Thời gian tạo offline (nếu có)
        /// </summary>
        public DateTime? OfflineCreatedAt { get; set; }
        
        /// <summary>
        /// Dữ liệu bổ sung (JSON)
        /// </summary>
        public string? AdditionalData { get; set; }
        
        // Navigation properties
        public virtual Event Event { get; set; } = null!;
        public virtual Guest Guest { get; set; } = null!;
        public virtual User? CheckedInByUser { get; set; }
        public virtual User? CheckedOutByUser { get; set; }
        
        /// <summary>
        /// Thời gian ở trong sự kiện (nếu đã check-out)
        /// </summary>
        public TimeSpan? Duration => CheckOutTime.HasValue && CheckInTime.HasValue 
            ? CheckOutTime.Value - CheckInTime.Value 
            : null;
    }
    
    /// <summary>
    /// Enum trạng thái check-in
    /// </summary>
    public enum CheckInStatus
    {
        /// <summary>
        /// Đã check-in
        /// </summary>
        CheckedIn = 0,
        
        /// <summary>
        /// Đã check-out
        /// </summary>
        CheckedOut = 1,
        
        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 2,
        
        /// <summary>
        /// Đang chờ xử lý
        /// </summary>
        Pending = 3
    }
    
    /// <summary>
    /// Enum loại check-in
    /// </summary>
    public enum CheckInType
    {
        /// <summary>
        /// Quét mã QR
        /// </summary>
        QRCode = 0,
        
        /// <summary>
        /// Check-in thủ công
        /// </summary>
        Manual = 1,
        
        /// <summary>
        /// Tự động (NFC, RFID)
        /// </summary>
        Automatic = 2,
        
        /// <summary>
        /// Bulk check-in
        /// </summary>
        Bulk = 3
    }
} 