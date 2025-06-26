using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Repositories
{
    public class TenantRepository : BaseRepository<Tenant>, ITenantRepository
    {
        public TenantRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<Tenant?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Email == email && !t.IsDeleted);
        }

        public async Task<bool> IsEmailTakenAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(t => t.Email == email && !t.IsDeleted);
            
            if (excludeId.HasValue)
                query = query.Where(t => t.Id != excludeId.Value);
                
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
        {
            return await _dbSet
                .Where(t => t.IsActive && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> UpdatePlanAsync(int tenantId, ServicePlan plan, DateTime expiryDate)
        {
            var tenant = await GetByIdAsync(tenantId);
            if (tenant == null)
                return false;

            tenant.CurrentPlan = plan;
            tenant.PlanExpiryDate = expiryDate;
            tenant.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(tenant);
            return true;
        }

        public async Task<Tenant?> GetBySubdomainAsync(string subdomain)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && !t.IsDeleted);
        }

        public async Task<Tenant> CreateAsync(Tenant tenant)
        {
            return await AddAsync(tenant);
        }

        public async Task<(IEnumerable<Tenant> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string searchTerm = "")
        {
            var query = _dbSet.Where(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Name.Contains(searchTerm) || t.Email.Contains(searchTerm));
            }

            var total = await query.CountAsync();
            var items = await query.OrderBy(t => t.Name)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return (items, total);
        }
    }
} 