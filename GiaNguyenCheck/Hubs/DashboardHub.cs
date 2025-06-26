using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using GiaNguyenCheck.DTOs;

namespace GiaNguyenCheck.Hubs
{
    [Authorize]
    public class DashboardHub : Hub
    {
        public async Task JoinTenantGroup(int tenantId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
        }

        public async Task LeaveTenantGroup(int tenantId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
        }

        public async Task JoinEventGroup(int eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"event_{eventId}");
        }

        public async Task LeaveEventGroup(int eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event_{eventId}");
        }

        public async Task SendDashboardUpdate(int tenantId, DashboardStatsDto stats)
        {
            await Clients.Group($"tenant_{tenantId}").SendAsync("ReceiveDashboardUpdate", stats);
        }

        public async Task SendEventUpdate(int eventId, EventStatsDto stats)
        {
            await Clients.Group($"event_{eventId}").SendAsync("ReceiveEventUpdate", stats);
        }

        public async Task SendCheckInNotification(int eventId, CheckInDto checkIn)
        {
            await Clients.Group($"event_{eventId}").SendAsync("ReceiveCheckIn", checkIn);
        }

        public async Task SendPaymentNotification(int tenantId, PaymentDto payment)
        {
            await Clients.Group($"tenant_{tenantId}").SendAsync("ReceivePayment", payment);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
} 