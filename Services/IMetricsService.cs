using System;

namespace GiaNguyenCheck.Services
{
    public interface IMetricsService
    {
        void RecordCheckIn(string eventType, double duration);
        void RecordPayment(string method, decimal amount, bool success);
        void RecordEventCreation(string eventType);
        void RecordGuestRegistration(string guestType);
        void RecordEmailSent(string emailType, bool success);
        void RecordApiCall(string endpoint, double duration, bool success);
        void RecordDatabaseQuery(string queryType, double duration);
        void RecordCacheHit(string cacheType, string key);
        void RecordCacheMiss(string cacheType, string key);
        void RecordError(string errorType, string message);
        void RecordTenantActivity(int tenantId, string activity);
    }
} 