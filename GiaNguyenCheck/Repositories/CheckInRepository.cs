using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Repositories
{
    public class CheckInRepository : BaseRepository<CheckIn>, ICheckInRepository
    {
        public CheckInRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CheckIn>> GetByEventIdAsync(int eventId)
        {
            return await _dbSet
                .Where(c => c.EventId == eventId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<CheckIn>> GetByGuestIdAsync(int guestId)
        {
            return await _dbSet
                .Where(c => c.GuestId == guestId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<CheckIn?> GetLatestByGuestIdAsync(int guestId)
        {
            return await _dbSet
                .Where(c => c.GuestId == guestId && !c.IsDeleted)
                .OrderByDescending(c => c.CheckInTime)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CheckIn>> GetByDateRangeAsync(int eventId, DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(c => c.EventId == eventId && 
                           !c.IsDeleted &&
                           c.CheckInTime.HasValue &&
                           c.CheckInTime >= from && 
                           c.CheckInTime <= to)
                .ToListAsync();
        }

        public async Task<IEnumerable<CheckIn>> GetUnsyncedAsync()
        {
            return await _dbSet
                .Where(c => !c.IsSynced && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> MarkAsSyncedAsync(IEnumerable<int> checkInIds)
        {
            try
            {
                var checkIns = await _dbSet
                    .Where(c => checkInIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var checkIn in checkIns)
                {
                    checkIn.IsSynced = true;
                    checkIn.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<DateTime, int>> GetCheckInStatsByHourAsync(int eventId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var checkIns = await _dbSet
                .Where(c => c.EventId == eventId && 
                           !c.IsDeleted &&
                           c.CheckInTime.HasValue &&
                           c.CheckInTime >= startOfDay && 
                           c.CheckInTime < endOfDay)
                .ToListAsync();

            return checkIns
                .Where(c => c.CheckInTime.HasValue)
                .GroupBy(c => new DateTime(c.CheckInTime!.Value.Year, c.CheckInTime!.Value.Month, c.CheckInTime!.Value.Day, c.CheckInTime!.Value.Hour, 0, 0))
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<int> GetTotalCheckInsCountAsync(int eventId)
        {
            return await _dbSet
                .CountAsync(c => c.EventId == eventId && !c.IsDeleted);
        }
    }
} 