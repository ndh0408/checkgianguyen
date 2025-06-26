using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using GiaNguyenCheck.Constants;
using GiaNguyenCheck.Interfaces;
using System.Security.Claims;
using GiaNguyenCheck.DTOs;

namespace GiaNguyenCheck.Hubs
{
    /// <summary>
    /// SignalR Hub cho real-time check-in notifications
    /// </summary>
    [Authorize]
    public class CheckInHub : Hub
    {
        private readonly ILogger<CheckInHub> _logger;
        private readonly ITenantProvider _tenantProvider;
        
        public CheckInHub(ILogger<CheckInHub> logger, ITenantProvider tenantProvider)
        {
            _logger = logger;
            _tenantProvider = tenantProvider;
        }
        
        /// <summary>
        /// Xử lý khi client kết nối
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var tenantId = GetTenantIdFromContext();
            
            _logger.LogInformation("User {UserId} từ Tenant {TenantId} đã kết nối SignalR", userId, tenantId);
            
            // Thêm user vào group theo tenant
            if (tenantId != Guid.Empty)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
            }
            
            // Thêm vào group theo role
            var role = GetUserRole();
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
            }
            
            await base.OnConnectedAsync();
        }
        
        /// <summary>
        /// Xử lý khi client ngắt kết nối
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var tenantId = GetTenantIdFromContext();
            
            _logger.LogInformation("User {UserId} từ Tenant {TenantId} đã ngắt kết nối SignalR", userId, tenantId);
            
            if (exception != null)
            {
                _logger.LogError(exception, "Lỗi khi ngắt kết nối SignalR cho User {UserId}", userId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        
        /// <summary>
        /// Join vào room của một sự kiện cụ thể
        /// </summary>
        public async Task JoinEventRoom(string eventId)
        {
            try
            {
                if (Guid.TryParse(eventId, out var parsedEventId))
                {
                    var groupName = $"Event_{parsedEventId}";
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    
                    _logger.LogDebug("User {UserId} đã join Event room: {EventId}", 
                        Context.UserIdentifier, eventId);
                    
                    // Thông báo cho client
                    await Clients.Caller.SendAsync("JoinedEventRoom", eventId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi join Event room: {EventId}", eventId);
            }
        }
        
        /// <summary>
        /// Leave khỏi room của một sự kiện
        /// </summary>
        public async Task LeaveEventRoom(string eventId)
        {
            try
            {
                if (Guid.TryParse(eventId, out var parsedEventId))
                {
                    var groupName = $"Event_{parsedEventId}";
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                    
                    _logger.LogDebug("User {UserId} đã leave Event room: {EventId}", 
                        Context.UserIdentifier, eventId);
                    
                    // Thông báo cho client
                    await Clients.Caller.SendAsync("LeftEventRoom", eventId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi leave Event room: {EventId}", eventId);
            }
        }
        
        /// <summary>
        /// Gửi message đến tất cả staff của một sự kiện
        /// </summary>
        public async Task SendMessageToEventStaff(string eventId, string message)
        {
            try
            {
                // Chỉ Event Manager mới có thể gửi message
                var role = GetUserRole();
                if (role != AppConstants.Roles.EventManager && role != AppConstants.Roles.TenantAdmin)
                {
                    await Clients.Caller.SendAsync("Error", "Bạn không có quyền gửi tin nhắn");
                    return;
                }
                
                if (Guid.TryParse(eventId, out var parsedEventId))
                {
                    var groupName = $"Event_{parsedEventId}";
                    var senderName = GetUserFullName();
                    
                    await Clients.Group(groupName).SendAsync("ReceiveMessage", new
                    {
                        EventId = eventId,
                        Sender = senderName,
                        Message = message,
                        Timestamp = DateTime.UtcNow
                    });
                    
                    _logger.LogInformation("Message sent to Event {EventId} staff by {UserId}", 
                        eventId, Context.UserIdentifier);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi message đến Event staff: {EventId}", eventId);
            }
        }
        
        /// <summary>
        /// Ping để kiểm tra kết nối
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }
        
        /// <summary>
        /// Lấy Tenant ID từ context
        /// </summary>
        private Guid GetTenantIdFromContext()
        {
            var tenantClaim = Context.User?.FindFirst(AppConstants.Claims.TenantId);
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                return tenantId;
            }
            return Guid.Empty;
        }
        
        /// <summary>
        /// Lấy role của user
        /// </summary>
        private string GetUserRole()
        {
            return Context.User?.FindFirst(AppConstants.Claims.Role)?.Value ?? string.Empty;
        }
        
        /// <summary>
        /// Lấy tên đầy đủ của user
        /// </summary>
        private string GetUserFullName()
        {
            return Context.User?.FindFirst(AppConstants.Claims.FullName)?.Value ?? "Unknown User";
        }

        public async Task JoinEventGroup(int eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"event_{eventId}");
        }

        public async Task LeaveEventGroup(int eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event_{eventId}");
        }

        public async Task SendCheckInUpdate(int eventId, CheckInDto checkIn)
        {
            await Clients.Group($"event_{eventId}").SendAsync("ReceiveCheckIn", checkIn);
        }

        public async Task SendCheckInStats(int eventId, CheckInStatsDto stats)
        {
            await Clients.Group($"event_{eventId}").SendAsync("ReceiveCheckInStats", stats);
        }
    }
} 