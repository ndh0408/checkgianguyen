using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GiaNguyenCheck.Services.BackgroundJobs;

namespace GiaNguyenCheck.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IEmailService _emailService;
        private readonly ICheckInService _checkInService;
        private readonly IPaymentService _paymentService;
        private readonly EmailJob _emailJob;

        public BackgroundJobService(
            ILogger<BackgroundJobService> logger,
            IEmailService emailService,
            ICheckInService checkInService,
            IPaymentService paymentService,
            EmailJob emailJob)
        {
            _logger = logger;
            _emailService = emailService;
            _checkInService = checkInService;
            _paymentService = paymentService;
            _emailJob = emailJob;
        }

        public void EnqueueEmailJob(int guestId, EmailType type)
        {
            _logger.LogInformation("Enqueuing email job for guest {GuestId}, type: {EmailType}", guestId, type);
            
            switch (type)
            {
                case EmailType.Invitation:
                    BackgroundJob.Enqueue(() => _emailJob.SendInvitationEmail(guestId));
                    break;
                case EmailType.Reminder:
                    BackgroundJob.Enqueue(() => _emailJob.SendEventReminder(guestId));
                    break;
                case EmailType.Confirmation:
                    BackgroundJob.Enqueue(() => _emailJob.SendCheckInConfirmation(guestId));
                    break;
                case EmailType.PaymentSuccess:
                    BackgroundJob.Enqueue(() => _emailJob.SendPaymentConfirmation(guestId));
                    break;
                case EmailType.PaymentFailed:
                    BackgroundJob.Enqueue(() => _emailJob.SendPaymentConfirmation(guestId));
                    break;
                case EmailType.EventCancelled:
                    BackgroundJob.Enqueue(() => _emailJob.SendEventReminder(guestId));
                    break;
            }
        }

        public void ScheduleEventReminder(int eventId, DateTime reminderTime)
        {
            _logger.LogInformation("Scheduling event reminder for event {EventId} at {ReminderTime}", eventId, reminderTime);
            
            var delay = reminderTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                BackgroundJob.Schedule(() => _emailJob.SendEventReminder(eventId), delay);
            }
            else
            {
                // If reminder time has passed, enqueue immediately
                BackgroundJob.Enqueue(() => _emailJob.SendEventReminder(eventId));
            }
        }

        public void ProcessBulkCheckIn(List<int> guestIds)
        {
            _logger.LogInformation("Processing bulk check-in for {Count} guests", guestIds.Count);
            
            // Process each guest in separate jobs with rate limiting
            foreach (var guestId in guestIds)
            {
                BackgroundJob.Enqueue<ICheckInService>(service => 
                    service.ProcessManualCheckInAsync(guestId, 0));
            }
        }

        public void ProcessPaymentCallback(string paymentId, string status)
        {
            _logger.LogInformation("Processing payment callback for {PaymentId} with status {Status}", paymentId, status);
            
            BackgroundJob.Enqueue<IPaymentService>(service => 
                service.ProcessCallbackAsync(paymentId, status));
        }

        public void GenerateEventReport(int eventId)
        {
            _logger.LogInformation("Generating report for event {EventId}", eventId);
            
            BackgroundJob.Enqueue<IDashboardService>(service => 
                service.GenerateEventReportAsync(eventId));
        }

        public void CleanupExpiredData()
        {
            _logger.LogInformation("Starting cleanup of expired data");
            
            // Schedule cleanup job to run daily at 2 AM
            RecurringJob.AddOrUpdate<IDashboardService>(
                "cleanup-expired-data",
                service => service.CleanupExpiredDataAsync(),
                Cron.Daily(2));
        }

        public void ScheduleRecurringJobs()
        {
            // Schedule daily cleanup
            RecurringJob.AddOrUpdate<IDashboardService>(
                "daily-cleanup",
                service => service.CleanupExpiredDataAsync(),
                Cron.Daily(2));

            // Schedule hourly health check
            RecurringJob.AddOrUpdate<IDashboardService>(
                "hourly-health-check",
                service => service.PerformHealthCheckAsync(),
                Cron.Hourly());

            // Schedule daily metrics aggregation
            RecurringJob.AddOrUpdate<IDashboardService>(
                "daily-metrics",
                service => service.AggregateDailyMetricsAsync(),
                Cron.Daily(1));

            // Schedule weekly report generation
            RecurringJob.AddOrUpdate<IDashboardService>(
                "weekly-reports",
                service => service.GenerateWeeklyReportsAsync(),
                Cron.Weekly(DayOfWeek.Monday, 3));

            _logger.LogInformation("Recurring jobs scheduled successfully");
        }

        public void EnqueueBulkEmailJob(int eventId, string emailType)
        {
            _logger.LogInformation("Enqueuing bulk email job for event {EventId}, type: {EmailType}", eventId, emailType);
            
            BackgroundJob.Enqueue(() => _emailJob.SendBulkEmails(eventId, emailType));
        }

        public void SchedulePaymentReminder(int paymentId, DateTime reminderTime)
        {
            _logger.LogInformation("Scheduling payment reminder for payment {PaymentId} at {ReminderTime}", paymentId, reminderTime);
            
            var delay = reminderTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                BackgroundJob.Schedule<IPaymentService>(service => 
                    service.SendPaymentReminderAsync(paymentId), delay);
            }
        }

        public void EnqueueDataSyncJob(int tenantId)
        {
            _logger.LogInformation("Enqueuing data sync job for tenant {TenantId}", tenantId);
            
            BackgroundJob.Enqueue<ITenantManagementService>(service => 
                service.SyncTenantDataAsync(tenantId));
        }

        public void ScheduleTenantBackup(int tenantId, DateTime backupTime)
        {
            _logger.LogInformation("Scheduling backup for tenant {TenantId} at {BackupTime}", tenantId, backupTime);
            
            var delay = backupTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                BackgroundJob.Schedule<ITenantManagementService>(service => 
                    service.BackupTenantDataAsync(tenantId), delay);
            }
        }
    }
} 