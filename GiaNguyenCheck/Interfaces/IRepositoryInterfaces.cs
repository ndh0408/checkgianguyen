using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Entities;

namespace GiaNguyenCheck.Interfaces
{
    /// <summary>
    /// Interface cơ sở cho repository pattern
    /// </summary>
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<(IEnumerable<TEntity>, int)> GetPagedAsync(int page, int pageSize);
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
    
    /// <summary>
    /// Interface cho Tenant Repository
    /// </summary>
    public interface ITenantRepository : IBaseRepository<Tenant>
    {
        Task<Tenant?> GetByEmailAsync(string email);
        Task<Tenant?> GetBySubdomainAsync(string subdomain);
        Task<Tenant> CreateAsync(Tenant tenant);
        Task<(IEnumerable<Tenant> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string searchTerm = "");
        Task<bool> IsEmailTakenAsync(string email, int? excludeId = null);
        Task<IEnumerable<Tenant>> GetActiveTenantsAsync();
        Task<bool> UpdatePlanAsync(int tenantId, ServicePlan plan, DateTime expiryDate);
    }
    
    /// <summary>
    /// Interface cho User Repository
    /// </summary>
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByTenantIdAsync(int tenantId);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> SetActiveStatusAsync(int userId, bool isActive);
        Task<DTOs.ApiResponse<User>> CreateAsync(User user, string password);
        Task<bool> AssignRoleAsync(int userId, string roleName);
    }
    
    /// <summary>
    /// Interface cho Event Repository
    /// </summary>
    public interface IEventRepository : IBaseRepository<Event>
    {
        Task<IEnumerable<Event>> GetByTenantIdAsync(int tenantId);
        Task<IEnumerable<Event>> GetByStatusAsync(Entities.EventStatus status);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync(int tenantId);
        Task<IEnumerable<Event>> GetActiveEventsAsync(int tenantId);
        Task<Event?> GetWithStatsAsync(int id);
        Task<bool> CanCreateEventAsync(int tenantId, ServicePlan plan);
    }
    
    /// <summary>
    /// Interface cho Guest Repository
    /// </summary>
    public interface IGuestRepository : IBaseRepository<Guest>
    {
        Task<IEnumerable<Guest>> GetByEventIdAsync(int eventId);
        Task<Guest?> GetByEmailAsync(int eventId, string email);
        Task<Guest?> GetByQRCodeAsync(string qrCodeHash);
        Task<IEnumerable<Guest>> GetByTypeAsync(int eventId, Entities.GuestType type);
        Task<bool> IsEmailTakenInEventAsync(int eventId, string email, int? excludeId = null);
        Task<int> GetTotalGuestsCountAsync(int eventId);
        Task<int> GetCheckedInCountAsync(int eventId);
        Task<IEnumerable<Guest>> SearchAsync(int eventId, string searchTerm);
        Task<bool> BulkAddAsync(IEnumerable<Guest> guests);
    }
    
    /// <summary>
    /// Interface cho CheckIn Repository
    /// </summary>
    public interface ICheckInRepository : IBaseRepository<CheckIn>
    {
        Task<IEnumerable<CheckIn>> GetByEventIdAsync(int eventId);
        Task<IEnumerable<CheckIn>> GetByGuestIdAsync(int guestId);
        Task<CheckIn?> GetLatestByGuestIdAsync(int guestId);
        Task<IEnumerable<CheckIn>> GetByDateRangeAsync(int eventId, DateTime from, DateTime to);
        Task<IEnumerable<CheckIn>> GetUnsyncedAsync();
        Task<bool> MarkAsSyncedAsync(IEnumerable<int> checkInIds);
        Task<Dictionary<DateTime, int>> GetCheckInStatsByHourAsync(int eventId, DateTime date);
        Task<int> GetTotalCheckInsCountAsync(int eventId);
    }
    
    /// <summary>
    /// Interface cho Payment Repository
    /// </summary>
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByTenantIdAsync(int tenantId);
        Task<Payment?> GetByReferenceCodeAsync(string referenceCode);
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
        Task<IEnumerable<Payment>> GetExpiredPaymentsAsync();
        Task<bool> UpdateStatusAsync(int paymentId, PaymentStatus status, string? transactionId = null);
    }
} 