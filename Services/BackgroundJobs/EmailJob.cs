using GiaNguyenCheck.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services.BackgroundJobs
{
    public class EmailJob
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailJob> _logger;
        private readonly IMetricsService _metricsService;

        public EmailJob(
            IEmailService emailService,
            ILogger<EmailJob> logger,
            IMetricsService metricsService)
        {
            _emailService = emailService;
            _logger = logger;
            _metricsService = metricsService;
        }

        public async Task SendInvitationEmail(int guestId)
        {
            try
            {
                _logger.LogInformation("Processing invitation email for guest {GuestId}", guestId);
                
                var startTime = DateTime.UtcNow;
                var success = await _emailService.SendInvitationEmailAsync(guestId);
                var duration = (DateTime.UtcNow - startTime).TotalSeconds;

                _metricsService.RecordEmailSent("Invitation", success);
                _metricsService.RecordApiCall("SendInvitationEmail", duration, success);

                if (success)
                {
                    _logger.LogInformation("Invitation email sent successfully for guest {GuestId}", guestId);
                }
                else
                {
                    _logger.LogWarning("Failed to send invitation email for guest {GuestId}", guestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invitation email for guest {GuestId}", guestId);
                _metricsService.RecordError("EmailJob", ex.Message);
                throw;
            }
        }

        public async Task SendEventReminder(int eventId)
        {
            try
            {
                _logger.LogInformation("Processing event reminder for event {EventId}", eventId);
                
                var startTime = DateTime.UtcNow;
                var success = await _emailService.SendEventReminderAsync(eventId);
                var duration = (DateTime.UtcNow - startTime).TotalSeconds;

                _metricsService.RecordEmailSent("Reminder", success);
                _metricsService.RecordApiCall("SendEventReminder", duration, success);

                if (success)
                {
                    _logger.LogInformation("Event reminder sent successfully for event {EventId}", eventId);
                }
                else
                {
                    _logger.LogWarning("Failed to send event reminder for event {EventId}", eventId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event reminder for event {EventId}", eventId);
                _metricsService.RecordError("EmailJob", ex.Message);
                throw;
            }
        }

        public async Task SendCheckInConfirmation(int guestId)
        {
            try
            {
                _logger.LogInformation("Processing check-in confirmation for guest {GuestId}", guestId);
                
                var startTime = DateTime.UtcNow;
                var success = await _emailService.SendCheckInConfirmationAsync(guestId);
                var duration = (DateTime.UtcNow - startTime).TotalSeconds;

                _metricsService.RecordEmailSent("CheckInConfirmation", success);
                _metricsService.RecordApiCall("SendCheckInConfirmation", duration, success);

                if (success)
                {
                    _logger.LogInformation("Check-in confirmation sent successfully for guest {GuestId}", guestId);
                }
                else
                {
                    _logger.LogWarning("Failed to send check-in confirmation for guest {GuestId}", guestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing check-in confirmation for guest {GuestId}", guestId);
                _metricsService.RecordError("EmailJob", ex.Message);
                throw;
            }
        }

        public async Task SendPaymentConfirmation(int guestId)
        {
            try
            {
                _logger.LogInformation("Processing payment confirmation for guest {GuestId}", guestId);
                
                var startTime = DateTime.UtcNow;
                var success = await _emailService.SendPaymentConfirmationAsync(guestId);
                var duration = (DateTime.UtcNow - startTime).TotalSeconds;

                _metricsService.RecordEmailSent("PaymentConfirmation", success);
                _metricsService.RecordApiCall("SendPaymentConfirmation", duration, success);

                if (success)
                {
                    _logger.LogInformation("Payment confirmation sent successfully for guest {GuestId}", guestId);
                }
                else
                {
                    _logger.LogWarning("Failed to send payment confirmation for guest {GuestId}", guestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment confirmation for guest {GuestId}", guestId);
                _metricsService.RecordError("EmailJob", ex.Message);
                throw;
            }
        }

        public async Task SendBulkEmails(int eventId, string emailType)
        {
            try
            {
                _logger.LogInformation("Processing bulk emails for event {EventId}, type: {EmailType}", eventId, emailType);
                
                var startTime = DateTime.UtcNow;
                var success = await _emailService.SendBulkEmailsAsync(eventId, emailType);
                var duration = (DateTime.UtcNow - startTime).TotalSeconds;

                _metricsService.RecordEmailSent($"Bulk{emailType}", success);
                _metricsService.RecordApiCall("SendBulkEmails", duration, success);

                if (success)
                {
                    _logger.LogInformation("Bulk emails sent successfully for event {EventId}, type: {EmailType}", eventId, emailType);
                }
                else
                {
                    _logger.LogWarning("Failed to send bulk emails for event {EventId}, type: {EmailType}", eventId, emailType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk emails for event {EventId}, type: {EmailType}", eventId, emailType);
                _metricsService.RecordError("EmailJob", ex.Message);
                throw;
            }
        }
    }
} 