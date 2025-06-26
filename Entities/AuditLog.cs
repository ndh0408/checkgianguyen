using System;
using System.ComponentModel.DataAnnotations;

namespace GiaNguyenCheck.Entities
{
    public class AuditLog : BaseEntity
    {
        [Required]
        public int TenantId { get; set; }
        
        public int? UserId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; }
        
        [Required]
        public int EntityId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Action { get; set; }
        
        public string? OldValues { get; set; }
        
        public string? NewValues { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Tenant Tenant { get; set; }
        public User? User { get; set; }
    }
} 