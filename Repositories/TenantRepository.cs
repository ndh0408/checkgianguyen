using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Constants;

namespace GiaNguyenCheck.Repositories;

/// <summary>
/// Repository for Tenant management
/// </summary>
public class TenantRepository : BaseRepository<Tenant>, ITenantRepository
{
    public TenantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.ContactEmail.ToLower() == email.ToLower());
    }

    public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeId = null)
    {
        var query = _dbSet.Where(t => t.ContactEmail.ToLower() == email.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
    {
        return await _dbSet
            .Where(t => t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<bool> UpdatePlanAsync(Guid tenantId, ServicePlan plan, DateTime expiresAt)
    {
        var tenant = await GetByIdAsync(tenantId);
        if (tenant == null) return false;

        tenant.ServicePlan = plan;
        tenant.PlanExpiresAt = expiresAt;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<ServicePlan, int>> GetTenantCountByPlanAsync()
    {
        return await _dbSet
            .Where(t => t.IsActive && !t.IsDeleted)
            .GroupBy(t => t.ServicePlan)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<Tenant>> GetExpiringTenantsAsync(int daysFromNow = 7)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysFromNow);
        
        return await _dbSet
            .Where(t => t.IsActive && 
                       !t.IsDeleted && 
                       t.PlanExpiresAt.HasValue && 
                       t.PlanExpiresAt.Value <= expiryDate)
            .OrderBy(t => t.PlanExpiresAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Tenant>, int)> SearchTenantsAsync(
        string? searchTerm = null, 
        ServicePlan? plan = null,
        bool? isActive = null,
        int page = 1, 
        int pageSize = 20)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => 
                t.Name.Contains(searchTerm) || 
                t.ContactEmail.Contains(searchTerm) ||
                t.ContactPhone.Contains(searchTerm));
        }

        if (plan.HasValue)
        {
            query = query.Where(t => t.ServicePlan == plan.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<TenantStatsDto> GetTenantStatsAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Events)
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return new TenantStatsDto();
        }

        var totalGuests = await _context.Guests
            .Where(g => g.Event.TenantId == tenantId)
            .CountAsync();

        var totalCheckIns = await _context.CheckIns
            .Where(c => c.Guest.Event.TenantId == tenantId)
            .CountAsync();

        var activeEvents = await _context.Events
            .Where(e => e.TenantId == tenantId && 
                       e.Status == EventStatus.Active)
            .CountAsync();

        return new TenantStatsDto
        {
            TenantId = tenantId,
            TotalEvents = tenant.Events.Count,
            ActiveEvents = activeEvents,
            TotalUsers = tenant.Users.Count,
            TotalGuests = totalGuests,
            TotalCheckIns = totalCheckIns,
            ServicePlan = tenant.ServicePlan,
            PlanExpiresAt = tenant.PlanExpiresAt,
            IsActive = tenant.IsActive
        };
    }
} 