using System;
using System.Collections.Generic;

namespace GiaNguyenCheck.Services
{
    public interface IBackgroundJobService
    {
        void EnqueueEmailJob(int guestId, EmailType type);
        void ScheduleEventReminder(int eventId, DateTime reminderTime);
        void ProcessBulkCheckIn(List<int> guestIds);
        void ProcessPaymentCallback(string paymentId, string status);
        void GenerateEventReport(int eventId);
        void CleanupExpiredData();
    }

    public enum EmailType
    {
        Invitation,
        Reminder,
        Confirmation,
        PaymentSuccess,
        PaymentFailed,
        EventCancelled
    }
} 