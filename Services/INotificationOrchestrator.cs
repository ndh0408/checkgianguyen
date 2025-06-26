using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public interface INotificationOrchestrator
    {
        Task SendSmartReminders(int eventId);
        Task SendPersonalizedNotifications(int guestId, NotificationType type);
        Task ScheduleOptimalNotifications(int eventId);
        Task<DateTime> CalculateOptimalSendTime(int guestId, NotificationType type);
        Task<NotificationChannel> DeterminePreferredChannel(int guestId);
        Task TrackNotificationEngagement(int notificationId, NotificationAction action);
        Task<List<NotificationPreference>> GetGuestPreferences(int guestId);
    }

    public enum NotificationType
    {
        EventReminder,
        CheckInConfirmation,
        PaymentReminder,
        EventCancellation,
        SpecialOffer,
        WelcomeMessage,
        FollowUp
    }

    public enum NotificationChannel
    {
        Email,
        SMS,
        PushNotification,
        WhatsApp,
        Telegram
    }

    public enum NotificationAction
    {
        Sent,
        Delivered,
        Opened,
        Clicked,
        Unsubscribed,
        Bounced
    }

    public class NotificationPreference
    {
        public NotificationType Type { get; set; }
        public NotificationChannel PreferredChannel { get; set; }
        public TimeSpan PreferredTime { get; set; }
        public bool IsEnabled { get; set; }
        public int Frequency { get; set; } // Days between notifications
    }

    public class SmartNotification
    {
        public int GuestId { get; set; }
        public int EventId { get; set; }
        public NotificationType Type { get; set; }
        public NotificationChannel Channel { get; set; }
        public DateTime OptimalSendTime { get; set; }
        public string PersonalizedContent { get; set; }
        public Dictionary<string, object> DynamicData { get; set; }
        public int Priority { get; set; }
    }
} 