using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Repositories
{
    public class GuestRepository : BaseRepository<Guest>, IGuestRepository
    {
        public GuestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Guest>> GetByEventIdAsync(int eventId)
        {
            return await _dbSet
                .Where(g => g.EventId == eventId && !g.IsDeleted)
                .ToListAsync();
        }

        public async Task<Guest?> GetByEmailAsync(int eventId, string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(g => g.EventId == eventId && 
                                         g.Email == email && 
                                         !g.IsDeleted);
        }

        public async Task<Guest?> GetByQRCodeAsync(string qrCodeHash)
        {
            return await _dbSet
                .FirstOrDefaultAsync(g => g.QRCodeHash == qrCodeHash && !g.IsDeleted);
        }

        public async Task<IEnumerable<Guest>> GetByTypeAsync(int eventId, GuestType type)
        {
            return await _dbSet
                .Where(g => g.EventId == eventId && 
                           g.Type == type && 
                           !g.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsEmailTakenInEventAsync(int eventId, string email, int? excludeId = null)
        {
            var query = _dbSet.Where(g => g.EventId == eventId && 
                                         g.Email == email && 
                                         !g.IsDeleted);
            
            if (excludeId.HasValue)
                query = query.Where(g => g.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<int> GetTotalGuestsCountAsync(int eventId)
        {
            return await _dbSet
                .CountAsync(g => g.EventId == eventId && !g.IsDeleted);
        }

        public async Task<int> GetCheckedInCountAsync(int eventId)
        {
            return await _dbSet
                .CountAsync(g => g.EventId == eventId && 
                               g.IsCheckedIn && 
                               !g.IsDeleted);
        }

        public async Task<IEnumerable<Guest>> SearchAsync(int eventId, string searchTerm)
        {
            return await _dbSet
                .Where(g => g.EventId == eventId && 
                           !g.IsDeleted &&
                           (g.FirstName.Contains(searchTerm) || 
                            g.LastName.Contains(searchTerm) || 
                            g.Email.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<bool> BulkAddAsync(IEnumerable<Guest> guests)
        {
            try
            {
                var guestList = guests.ToList();
                var now = DateTime.UtcNow;

                foreach (var guest in guestList)
                {
                    guest.CreatedAt = now;
                    guest.IsDeleted = false;
                }

                _dbSet.AddRange(guestList);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 