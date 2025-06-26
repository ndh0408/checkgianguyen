using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Repositories
{
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Event>> GetByTenantIdAsync(int tenantId)
        {
            return await _dbSet
                .Where(e => e.TenantId == tenantId && !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetByStatusAsync(EventStatus status)
        {
            return await _dbSet
                .Where(e => e.Status == status && !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int tenantId)
        {
            return await _dbSet
                .Where(e => e.TenantId == tenantId && 
                           e.StartTime > DateTime.UtcNow && 
                           !e.IsDeleted)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetActiveEventsAsync(int tenantId)
        {
            return await _dbSet
                .Where(e => e.TenantId == tenantId && 
                           e.Status == EventStatus.Published && 
                           !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<Event?> GetWithStatsAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Guests)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<bool> CanCreateEventAsync(int tenantId, ServicePlan plan)
        {
            var eventCount = await _dbSet
                .CountAsync(e => e.TenantId == tenantId && !e.IsDeleted);

            // Basic limits - you can expand this based on plan
            var maxEvents = plan switch
            {
                ServicePlan.Free => 1,
                ServicePlan.Basic => 5,
                ServicePlan.Pro => 20,
                ServicePlan.Enterprise => int.MaxValue,
                _ => 1
            };

            return eventCount < maxEvents;
        }
    }
} 