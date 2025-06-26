using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Repositories;
using GiaNguyenCheck.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GiaNguyenCheck.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly ICheckInRepository _checkInRepository;
        private readonly ApplicationDbContext _context;

        public CheckInService(ICheckInRepository checkInRepository, ApplicationDbContext context)
        {
            _checkInRepository = checkInRepository;
            _context = context;
        }

        public async Task<List<CheckIn>> GetRecentCheckInsAsync(int limit)
        {
            return await _context.CheckIns
                .OrderByDescending(c => c.CheckInTime)
                .Include(c => c.Guest)
                .Include(c => c.Event)
                .Take(limit)
                .ToListAsync();
        }

        // Stub implementations for interface compatibility
        public Task<ApiResponse<CheckInDto>> CheckInGuestAsync(string qrCodeData, int checkedInByUserId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<CheckInDto>> CheckOutGuestAsync(int guestId, int eventId, int checkedOutByUserId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<CheckInDto>> GetCheckInAsync(int checkInId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<PagedResult<CheckInDto>>> GetCheckInsAsync(int eventId, int page = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<CheckInStatsDto>> GetCheckInStatsAsync(int eventId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<CheckInTrendDto>>> GetCheckInTrendsAsync(int eventId, int days = 7)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<CheckInHourlyStatsDto>>> GetHourlyCheckInStatsAsync(int eventId, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> SyncCheckInsAsync(int eventId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> ExportCheckInDataAsync(int eventId, string format = "csv")
        {
            throw new NotImplementedException();
        }
    }
} 