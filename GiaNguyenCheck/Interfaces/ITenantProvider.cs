namespace GiaNguyenCheck.Interfaces
{
    /// <summary>
    /// Interface cung cấp thông tin tenant hiện tại
    /// </summary>
    public interface ITenantProvider
    {
        /// <summary>
        /// Lấy ID tenant hiện tại
        /// </summary>
        /// <returns>Tenant ID</returns>
        int GetTenantId();
        
        /// <summary>
        /// Lấy ID tenant hiện tại
        /// </summary>
        /// <returns>Tenant ID</returns>
        int GetCurrentTenantId();
        
        /// <summary>
        /// Kiểm tra xem có tenant nào được set hay không
        /// </summary>
        /// <returns>True nếu có tenant</returns>
        bool HasTenant();
        
        /// <summary>
        /// Set tenant ID cho request hiện tại
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        void SetTenantId(int tenantId);
        
        /// <summary>
        /// Xóa tenant ID khỏi context hiện tại
        /// </summary>
        void ClearTenant();
        
        /// <summary>
        /// Kiểm tra xem user hiện tại có quyền truy cập resource của tenant không
        /// </summary>
        /// <param name="resourceTenantId">Tenant ID của resource</param>
        /// <returns>True nếu có quyền truy cập</returns>
        Task<bool> CanAccessResourceAsync(int resourceTenantId);
        
        /// <summary>
        /// Lấy thông tin tenant hiện tại
        /// </summary>
        /// <returns>Thông tin tenant</returns>
        Task<Entities.Tenant?> GetCurrentTenantAsync();
        
        /// <summary>
        /// Kiểm tra xem tenant hiện tại có được phép thực hiện action này không
        /// </summary>
        /// <param name="action">Action cần kiểm tra</param>
        /// <returns>True nếu được phép</returns>
        Task<bool> CanPerformActionAsync(string action);
        
        /// <summary>
        /// Lấy user ID hiện tại
        /// </summary>
        /// <returns>User ID</returns>
        int? GetCurrentUserId();
        
        /// <summary>
        /// Lấy role của user hiện tại
        /// </summary>
        /// <returns>Role name</returns>
        string? GetCurrentUserRole();
        
        Task SetTenantIdAsync(int tenantId);
        Task SetTenantBySubdomainAsync(string subdomain);
        string? GetCurrentSubdomain();
    }
} 