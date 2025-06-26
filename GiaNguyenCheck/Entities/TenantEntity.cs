namespace GiaNguyenCheck.Entities
{
    /// <summary>
    /// Interface đánh dấu entity thuộc về một tenant cụ thể
    /// </summary>
    public interface ITenantEntity
    {
        public int TenantId { get; set; }
    }
    
    /// <summary>
    /// Lớp cơ sở cho các entity có multi-tenant
    /// </summary>
    public abstract class TenantEntity : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }
    }
} 