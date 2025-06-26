using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Entity quản lý thanh toán gói dịch vụ
    /// </summary>
    public class Payment : BaseEntity
    {
        /// <summary>
        /// ID tenant thanh toán
        /// </summary>
        public int TenantId { get; set; }
        
        /// <summary>
        /// Gói dịch vụ được mua
        /// </summary>
        public ServicePlan Plan { get; set; }
        
        /// <summary>
        /// Tên gói dịch vụ
        /// </summary>
        [StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        /// <summary>
        /// Thời hạn gói (số tháng)
        /// </summary>
        public int DurationMonths { get; set; } = 1;
        
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
        public PaymentMethod Method { get; set; }

        /// <summary>
        /// Phương thức thanh toán (legacy)
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// Trạng thái thanh toán
        /// </summary>
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        
        /// <summary>
        /// Mã giao dịch từ hệ thống thanh toán
        /// </summary>
        [StringLength(200)]
        public string? TransactionId { get; set; }
        
        /// <summary>
        /// Mã tham chiếu nội bộ
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ReferenceCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Thông tin gateway thanh toán
        /// </summary>
        [StringLength(100)]
        public string? PaymentGateway { get; set; }
        
        /// <summary>
        /// Thời gian thanh toán thành công
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành thanh toán
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// Thời gian hết hạn thanh toán
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// Ngày bắt đầu có hiệu lực
        /// </summary>
        public DateTime ValidFrom { get; set; }
        
        /// <summary>
        /// Ngày hết hạn
        /// </summary>
        public DateTime ValidTo { get; set; }
        
        /// <summary>
        /// Tự động gia hạn
        /// </summary>
        public bool AutoRenew { get; set; } = false;
        
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
        
        /// <summary>
        /// Thông tin invoice
        /// </summary>
        [StringLength(200)]
        public string? InvoiceNumber { get; set; }
        
        /// <summary>
        /// URL invoice
        /// </summary>
        [StringLength(500)]
        public string? InvoiceUrl { get; set; }
        
        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual User? User { get; set; }
    }
    
    /// <summary>
    /// Enum phương thức thanh toán
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Momo
        /// </summary>
        Momo = 0,
        
        /// <summary>
        /// Stripe
        /// </summary>
        Stripe = 1,
        
        /// <summary>
        /// PayPal
        /// </summary>
        PayPal = 2,
        
        /// <summary>
        /// VNPAY
        /// </summary>
        VNPay = 3,
        
        /// <summary>
        /// Chuyển khoản ngân hàng
        /// </summary>
        BankTransfer = 4,
        
        /// <summary>
        /// Tiền mặt
        /// </summary>
        Cash = 5,
        
        /// <summary>
        /// Miễn phí
        /// </summary>
        Free = 6
    }
    
    /// <summary>
    /// Enum trạng thái thanh toán
    /// </summary>
    public enum PaymentStatus
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
        Success = 2,
        
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