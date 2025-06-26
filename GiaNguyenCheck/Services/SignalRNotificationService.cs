using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GiaNguyenCheck.Services
{
    public class SignalRNotificationService
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        public SignalRNotificationService(IHubContext<DashboardHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task NotifyCheckInAsync(int eventId, CheckInDto checkIn)
        {
            await _hubContext.Clients.Group($"event_{eventId}").SendAsync("ReceiveCheckIn", checkIn);
        }
        public async Task NotifyEventStatsUpdateAsync(int eventId, EventStatsDto stats)
        {
            await _hubContext.Clients.Group($"event_{eventId}").SendAsync("ReceiveEventStats", stats);
        }
    }
} 