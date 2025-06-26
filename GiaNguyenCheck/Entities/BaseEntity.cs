using System.ComponentModel.DataAnnotations;

namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Lớp cơ sở cho tất cả các entities
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// ID chính của entity
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Ngày cập nhật cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Đánh dấu đã bị xóa (soft delete)
        /// </summary>
        public bool IsDeleted { get; set; } = false;
        
        /// <summary>
        /// Ngày xóa
        /// </summary>
        public DateTime? DeletedAt { get; set; }
        
        public string? CreatedBy { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        public string? DeletedBy { get; set; }
    }
} 