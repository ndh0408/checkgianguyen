using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Entity quản lý thanh toán sự kiện
    /// </summary>
    public class EventPayment : BaseEntity
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
        /// ID tenant
        /// </summary>
        public int TenantId { get; set; }
        
        /// <summary>
        /// Số tiền thanh toán
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Đơn vị tiền tệ
        /// </summary>
        [StringLength(3)]
        public string Currency { get; set; } = "VND";
        
        /// <summary>
        /// Phương thức thanh toán
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// Trạng thái thanh toán
        /// </summary>
        public EventPaymentStatus Status { get; set; } = EventPaymentStatus.Pending;
        
        /// <summary>
        /// Mã giao dịch từ hệ thống thanh toán
        /// </summary>
        [StringLength(200)]
        public string? TransactionId { get; set; }
        
        /// <summary>
        /// Thời gian thanh toán thành công
        /// </summary>
        public DateTime? PaidAt { get; set; }
        
        /// <summary>
        /// Thời gian hoàn tiền
        /// </summary>
        public DateTime? RefundedAt { get; set; }
        
        /// <summary>
        /// Số tiền hoàn lại
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundedAmount { get; set; }
        
        /// <summary>
        /// Mô tả thanh toán
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Thông tin phản hồi từ gateway (JSON)
        /// </summary>
        public string? GatewayResponse { get; set; }
        
        /// <summary>
        /// Lý do thất bại (nếu có)
        /// </summary>
        [StringLength(500)]
        public string? FailureReason { get; set; }
        
        // Navigation properties
        public virtual Event Event { get; set; } = null!;
        public virtual Guest Guest { get; set; } = null!;
    }
    
    /// <summary>
    /// Enum trạng thái thanh toán sự kiện
    /// </summary>
    public enum EventPaymentStatus
    {
        /// <summary>
        /// Đang chờ thanh toán
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Đang xử lý
        /// </summary>
        Processing = 1,
        
        /// <summary>
        /// Thành công
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Thất bại
        /// </summary>
        Failed = 3,
        
        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 4,
        
        /// <summary>
        /// Đã hoàn tiền
        /// </summary>
        Refunded = 5,
        
        /// <summary>
        /// Hết hạn
        /// </summary>
        Expired = 6
    }
} 