using GiaNguyenCheck.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public class NotificationOrchestrator : INotificationOrchestrator
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cacheService;
        private readonly IMetricsService _metricsService;
        private readonly ILogger<NotificationOrchestrator> _logger;

        public NotificationOrchestrator(
            IGuestRepository guestRepository,
            IEventRepository eventRepository,
            IEmailService emailService,
            ICacheService cacheService,
            IMetricsService metricsService,
            ILogger<NotificationOrchestrator> logger)
        {
            _guestRepository = guestRepository;
            _eventRepository = eventRepository;
            _emailService = emailService;
            _cacheService = cacheService;
            _metricsService = metricsService;
            _logger = logger;
        }

        public async Task SendSmartReminders(int eventId)
        {
            try
            {
                _logger.LogInformation("Starting smart reminders for event {EventId}", eventId);
                
                var guests = await GetGuestsNeedingReminder(eventId);
                var sentCount = 0;

                foreach (var guest in guests)
                {
                    try
                    {
                        var optimalTime = await CalculateOptimalSendTime(guest.Id, NotificationType.EventReminder);
                        var channel = await DeterminePreferredChannel(guest.Id);
                        
                        // Schedule notification at optimal time
                        BackgroundJob.Schedule(
                            () => SendPersonalizedNotification(guest.Id, NotificationType.EventReminder, channel),
                            optimalTime - DateTime.UtcNow
                        );

                        sentCount++;
                        _metricsService.RecordApiCall("SendSmartReminder", 0.1, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error scheduling reminder for guest {GuestId}", guest.Id);
                        _metricsService.RecordError("NotificationOrchestrator", ex.Message);
                    }
                }

                _logger.LogInformation("Scheduled {Count} smart reminders for event {EventId}", sentCount, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending smart reminders for event {EventId}", eventId);
                throw;
            }
        }

        public async Task SendPersonalizedNotifications(int guestId, NotificationType type)
        {
            try
            {
                var channel = await DeterminePreferredChannel(guestId);
                await SendPersonalizedNotification(guestId, type, channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending personalized notification for guest {GuestId}", guestId);
                throw;
            }
        }

        public async Task ScheduleOptimalNotifications(int eventId)
        {
            try
            {
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                if (eventEntity == null)
                    throw new ArgumentException($"Event {eventId} not found");

                var guests = await _guestRepository.GetByEventAsync(eventId);
                var notifications = new List<SmartNotification>();

                foreach (var guest in guests)
                {
                    var preferences = await GetGuestPreferences(guest.Id);
                    
                    foreach (var preference in preferences.Where(p => p.IsEnabled))
                    {
                        var optimalTime = await CalculateOptimalSendTime(guest.Id, preference.Type);
                        var channel = preference.PreferredChannel;
                        
                        var notification = new SmartNotification
                        {
                            GuestId = guest.Id,
                            EventId = eventId,
                            Type = preference.Type,
                            Channel = channel,
                            OptimalSendTime = optimalTime,
                            PersonalizedContent = await GeneratePersonalizedContent(guest, preference.Type),
                            DynamicData = await GetDynamicData(guest, eventEntity, preference.Type),
                            Priority = CalculatePriority(preference.Type, guest)
                        };

                        notifications.Add(notification);
                    }
                }

                // Sort by priority and schedule
                var sortedNotifications = notifications.OrderByDescending(n => n.Priority);
                
                foreach (var notification in sortedNotifications)
                {
                    var delay = notification.OptimalSendTime - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                    {
                        BackgroundJob.Schedule(
                            () => SendPersonalizedNotification(notification.GuestId, notification.Type, notification.Channel),
                            delay
                        );
                    }
                }

                _logger.LogInformation("Scheduled {Count} optimal notifications for event {EventId}", 
                    notifications.Count, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling optimal notifications for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<DateTime> CalculateOptimalSendTime(int guestId, NotificationType type)
        {
            var cacheKey = $"optimal_send_time_{guestId}_{type}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var baseTime = DateTime.UtcNow;
                var guest = await _guestRepository.GetByIdAsync(guestId);
                var preferences = await GetGuestPreferences(guestId);
                
                var preference = preferences.FirstOrDefault(p => p.Type == type);
                if (preference != null)
                {
                    // Apply preferred time
                    baseTime = baseTime.Date.Add(preference.PreferredTime);
                }

                // Type-specific adjustments
                switch (type)
                {
                    case NotificationType.EventReminder:
                        // Send 1 day before event
                        baseTime = baseTime.AddDays(-1);
                        break;
                    case NotificationType.PaymentReminder:
                        // Send 3 days before due date
                        baseTime = baseTime.AddDays(-3);
                        break;
                    case NotificationType.CheckInConfirmation:
                        // Send immediately
                        baseTime = DateTime.UtcNow;
                        break;
                    case NotificationType.SpecialOffer:
                        // Send during business hours
                        baseTime = baseTime.Date.AddHours(10); // 10 AM
                        break;
                }

                // Avoid sending during night hours (10 PM - 6 AM)
                if (baseTime.Hour >= 22 || baseTime.Hour < 6)
                {
                    baseTime = baseTime.Date.AddHours(9); // Move to 9 AM
                }

                return baseTime;
            }, TimeSpan.FromMinutes(30));
        }

        public async Task<NotificationChannel> DeterminePreferredChannel(int guestId)
        {
            var cacheKey = $"preferred_channel_{guestId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var preferences = await GetGuestPreferences(guestId);
                var enabledChannels = preferences.Where(p => p.IsEnabled).ToList();

                if (!enabledChannels.Any())
                    return NotificationChannel.Email; // Default fallback

                // Return the most preferred channel
                return enabledChannels.OrderByDescending(p => p.Frequency).First().PreferredChannel;
            }, TimeSpan.FromHours(1));
        }

        public async Task TrackNotificationEngagement(int notificationId, NotificationAction action)
        {
            try
            {
                _metricsService.RecordApiCall("TrackNotificationEngagement", 0.05, true);
                
                // Track engagement metrics
                switch (action)
                {
                    case NotificationAction.Opened:
                        _metricsService.RecordApiCall("NotificationOpened", 0.05, true);
                        break;
                    case NotificationAction.Clicked:
                        _metricsService.RecordApiCall("NotificationClicked", 0.05, true);
                        break;
                    case NotificationAction.Bounced:
                        _metricsService.RecordError("NotificationBounced", "Email bounced");
                        break;
                }

                _logger.LogDebug("Tracked notification engagement: {NotificationId}, {Action}", notificationId, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking notification engagement for {NotificationId}", notificationId);
            }
        }

        public async Task<List<NotificationPreference>> GetGuestPreferences(int guestId)
        {
            var cacheKey = $"guest_preferences_{guestId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Simulate guest preferences (in production, this would come from database)
                return new List<NotificationPreference>
                {
                    new NotificationPreference
                    {
                        Type = NotificationType.EventReminder,
                        PreferredChannel = NotificationChannel.Email,
                        PreferredTime = TimeSpan.FromHours(9), // 9 AM
                        IsEnabled = true,
                        Frequency = 1
                    },
                    new NotificationPreference
                    {
                        Type = NotificationType.CheckInConfirmation,
                        PreferredChannel = NotificationChannel.SMS,
                        PreferredTime = TimeSpan.FromHours(0), // Immediate
                        IsEnabled = true,
                        Frequency = 0
                    },
                    new NotificationPreference
                    {
                        Type = NotificationType.PaymentReminder,
                        PreferredChannel = NotificationChannel.Email,
                        PreferredTime = TimeSpan.FromHours(10), // 10 AM
                        IsEnabled = true,
                        Frequency = 3
                    },
                    new NotificationPreference
                    {
                        Type = NotificationType.SpecialOffer,
                        PreferredChannel = NotificationChannel.PushNotification,
                        PreferredTime = TimeSpan.FromHours(14), // 2 PM
                        IsEnabled = false, // Disabled by default
                        Frequency = 7
                    }
                };
            }, TimeSpan.FromHours(6));
        }

        private async Task<List<dynamic>> GetGuestsNeedingReminder(int eventId)
        {
            var cacheKey = $"guests_needing_reminder_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var guests = await _guestRepository.GetByEventAsync(eventId);
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                
                // Filter guests who need reminders
                return guests.Where(g => 
                    !g.IsConfirmed && 
                    g.InvitationStatus == InvitationStatus.Sent &&
                    eventEntity.StartTime > DateTime.UtcNow.AddDays(1)
                ).Cast<dynamic>().ToList();
            }, TimeSpan.FromMinutes(5));
        }

        private async Task SendPersonalizedNotification(int guestId, NotificationType type, NotificationChannel channel)
        {
            try
            {
                var guest = await _guestRepository.GetByIdAsync(guestId);
                if (guest == null)
                    throw new ArgumentException($"Guest {guestId} not found");

                var content = await GeneratePersonalizedContent(guest, type);
                
                switch (channel)
                {
                    case NotificationChannel.Email:
                        await _emailService.SendPersonalizedEmailAsync(guestId, type, content);
                        break;
                    case NotificationChannel.SMS:
                        // Implement SMS service
                        _logger.LogInformation("SMS notification would be sent to {Phone}", guest.Phone);
                        break;
                    case NotificationChannel.PushNotification:
                        // Implement push notification service
                        _logger.LogInformation("Push notification would be sent to guest {GuestId}", guestId);
                        break;
                    default:
                        await _emailService.SendPersonalizedEmailAsync(guestId, type, content);
                        break;
                }

                _metricsService.RecordApiCall($"Send{type}Notification", 0.1, true);
                _logger.LogInformation("Sent {Type} notification to guest {GuestId} via {Channel}", 
                    type, guestId, channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending personalized notification to guest {GuestId}", guestId);
                _metricsService.RecordError("NotificationOrchestrator", ex.Message);
                throw;
            }
        }

        private async Task<string> GeneratePersonalizedContent(dynamic guest, NotificationType type)
        {
            var guestName = $"{guest.FirstName} {guest.LastName}";
            
            switch (type)
            {
                case NotificationType.EventReminder:
                    return $"Xin chào {guestName}, đây là lời nhắc nhở về sự kiện sắp tới. Chúng tôi rất mong được gặp bạn!";
                case NotificationType.CheckInConfirmation:
                    return $"Xin chào {guestName}, bạn đã check-in thành công. Cảm ơn bạn đã tham gia!";
                case NotificationType.PaymentReminder:
                    return $"Xin chào {guestName}, vui lòng hoàn tất thanh toán để đảm bảo chỗ tham gia sự kiện.";
                case NotificationType.SpecialOffer:
                    return $"Xin chào {guestName}, chúng tôi có ưu đãi đặc biệt dành cho bạn!";
                default:
                    return $"Xin chào {guestName}, bạn có thông báo mới từ chúng tôi.";
            }
        }

        private async Task<Dictionary<string, object>> GetDynamicData(dynamic guest, dynamic eventEntity, NotificationType type)
        {
            return new Dictionary<string, object>
            {
                { "GuestName", $"{guest.FirstName} {guest.LastName}" },
                { "EventName", eventEntity.Name },
                { "EventDate", eventEntity.StartTime },
                { "EventLocation", eventEntity.Location },
                { "NotificationType", type.ToString() },
                { "Timestamp", DateTime.UtcNow }
            };
        }

        private int CalculatePriority(NotificationType type, dynamic guest)
        {
            var basePriority = type switch
            {
                NotificationType.CheckInConfirmation => 100,
                NotificationType.PaymentReminder => 80,
                NotificationType.EventReminder => 60,
                NotificationType.SpecialOffer => 40,
                _ => 20
            };

            // VIP guests get higher priority
            if (guest.IsVIP)
                basePriority += 20;

            return basePriority;
        }
    }
} 