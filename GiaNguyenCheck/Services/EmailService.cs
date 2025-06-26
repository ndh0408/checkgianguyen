using System.Net;
using System.Net.Mail;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GiaNguyenCheck.DTOs;
using System.Text;

namespace GiaNguyenCheck.Services
{
    /// <summary>
    /// Service để gửi email
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Cấu hình SMTP client
            _smtpClient = new SmtpClient
            {
                Host = _configuration["EmailSettings:Host"] ?? "smtp.gmail.com",
                Port = int.Parse(_configuration["EmailSettings:Port"] ?? "587"),
                EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true"),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Username"] ?? "",
                    _configuration["EmailSettings:Password"] ?? ""
                )
            };
        }

        /// <summary>
        /// Gửi email đơn giản
        /// </summary>
        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var from = _configuration["EmailSettings:FromEmail"] ?? "noreply@gianguyencheck.com";
                var fromName = _configuration["EmailSettings:FromName"] ?? "GiaNguyenCheck";

                using var message = new MailMessage();
                message.From = new MailAddress(from, fromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                await _smtpClient.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }

        // Interface methods implementation
        public async Task<bool> SendInvitationEmailAsync(Invitation invitation, string tenantName)
        {
            var subject = $"Mời tham gia hệ thống {tenantName}";
            var body = GenerateInvitationEmailTemplate(invitation, tenantName);
            
            return await SendEmailAsync(invitation.Email, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(User user, string tenantName)
        {
            var subject = $"Chào mừng đến với {tenantName}";
            var body = GenerateWelcomeEmailTemplate(user, tenantName);
            
            return await SendEmailAsync(user.Email, subject, body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(User user, string resetToken)
        {
            var subject = "Đặt lại mật khẩu";
            var resetUrl = $"{_configuration["AppUrl"]}/auth/reset-password?token={resetToken}";
            var body = GeneratePasswordResetTemplate(user, resetUrl);
            
            return await SendEmailAsync(user.Email, subject, body);
        }

        public async Task<bool> SendPaymentSuccessEmailAsync(Tenant tenant, Payment payment)
        {
            // Gửi email xác nhận thanh toán (mock)
            return await Task.FromResult(true);
        }

        public async Task<bool> SendEventReminderEmailAsync(Guest guest, Event eventEntity)
        {
            var subject = $"Nhắc nhở: Sự kiện {eventEntity.Name} sắp diễn ra";
            var body = GenerateEventReminderTemplate(guest, eventEntity);
            
            return await SendEmailAsync(guest.Email, subject, body);
        }

        public async Task<bool> SendBulkInvitationsAsync(IEnumerable<Guest> guests)
        {
            // Gửi email hàng loạt (mock)
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Gửi email mời tham gia sự kiện
        /// </summary>
        public async Task<bool> SendEventInvitationAsync(string to, string guestName, string eventName, 
            string eventDetails, string qrCodeUrl, string eventUrl)
        {
            var subject = $"Thư mời tham gia sự kiện: {eventName}";
            
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #667eea; }}
                        .event-details {{ background-color: #f8f9ff; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .qr-code {{ text-align: center; margin: 30px 0; }}
                        .qr-code img {{ max-width: 200px; }}
                        .button {{ display: inline-block; background-color: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>🎉 GiaNguyenCheck</div>
                            <h1>Thư mời tham gia sự kiện</h1>
                        </div>
                        
                        <p>Xin chào <strong>{guestName}</strong>,</p>
                        
                        <p>Bạn được mời tham gia sự kiện <strong>{eventName}</strong>. Chúng tôi rất vui mừng được đón tiếp bạn!</p>
                        
                        <div class='event-details'>
                            <h3>📋 Thông tin sự kiện:</h3>
                            <p>{eventDetails}</p>
                        </div>
                        
                        <div class='qr-code'>
                            <h3>📱 Mã QR check-in:</h3>
                            <p>Vui lòng lưu mã QR này để check-in nhanh chóng tại sự kiện:</p>
                            <img src='{qrCodeUrl}' alt='QR Code' />
                            <p><small>Hoặc sử dụng link: <a href='{eventUrl}'>Check-in trực tuyến</a></small></p>
                        </div>
                        
                        <div style='text-align: center;'>
                            <a href='{eventUrl}' class='button'>Xem chi tiết sự kiện</a>
                        </div>
                        
                        <div class='footer'>
                            <p>Cảm ơn bạn đã sử dụng GiaNguyenCheck!</p>
                            <p>Nếu có thắc mắc, vui lòng liên hệ: support@gianguyencheck.com</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(to, subject, body, true);
        }

        /// <summary>
        /// Gửi email xác nhận đăng ký
        /// </summary>
        public async Task<bool> SendRegistrationConfirmationAsync(string to, string userName, string confirmationUrl)
        {
            var subject = "Xác nhận đăng ký tài khoản GiaNguyenCheck";
            
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #667eea;'>🎉 Chào mừng đến với GiaNguyenCheck!</h1>
                        </div>
                        
                        <p>Xin chào <strong>{userName}</strong>,</p>
                        
                        <p>Cảm ơn bạn đã đăng ký tài khoản GiaNguyenCheck. Để hoàn tất quá trình đăng ký, vui lòng click vào nút bên dưới để xác nhận email:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationUrl}' style='display: inline-block; background-color: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px;'>Xác nhận email</a>
                        </div>
                        
                        <p>Hoặc copy link sau vào trình duyệt:</p>
                        <p style='word-break: break-all; background-color: #f8f9ff; padding: 10px; border-radius: 5px;'>{confirmationUrl}</p>
                        
                        <div style='text-align: center; margin-top: 30px; font-size: 12px; color: #666;'>
                            <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
                            <p>GiaNguyenCheck Team</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(to, subject, body, true);
        }

        /// <summary>
        /// Gửi email reset password
        /// </summary>
        public async Task<bool> SendPasswordResetAsync(string to, string userName, string resetUrl)
        {
            var subject = "Đặt lại mật khẩu GiaNguyenCheck";
            
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #667eea;'>🔐 Đặt lại mật khẩu</h1>
                        </div>
                        
                        <p>Xin chào <strong>{userName}</strong>,</p>
                        
                        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Click vào nút bên dưới để tạo mật khẩu mới:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetUrl}' style='display: inline-block; background-color: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px;'>Đặt lại mật khẩu</a>
                        </div>
                        
                        <p>Hoặc copy link sau vào trình duyệt:</p>
                        <p style='word-break: break-all; background-color: #f8f9ff; padding: 10px; border-radius: 5px;'>{resetUrl}</p>
                        
                        <p><strong>Lưu ý:</strong> Link này sẽ hết hạn sau 1 giờ vì lý do bảo mật.</p>
                        
                        <div style='text-align: center; margin-top: 30px; font-size: 12px; color: #666;'>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                            <p>GiaNguyenCheck Team</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(to, subject, body, true);
        }

        /// <summary>
        /// Gửi thông báo check-in thành công
        /// </summary>
        public async Task<bool> SendCheckInNotificationAsync(string to, string guestName, string eventName, DateTime checkInTime)
        {
            var subject = $"Check-in thành công - {eventName}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #28a745;'>✅ Check-in thành công!</h1>
                        </div>
                        
                        <p>Xin chào <strong>{guestName}</strong>,</p>
                        
                        <p>Bạn đã check-in thành công vào sự kiện <strong>{eventName}</strong> lúc {checkInTime:HH:mm} ngày {checkInTime:dd/MM/yyyy}.</p>
                        
                        <div style='background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 5px; padding: 15px; margin: 20px 0;'>
                            <h3 style='color: #155724; margin-top: 0;'>Thông tin check-in:</h3>
                            <p><strong>Sự kiện:</strong> {eventName}</p>
                            <p><strong>Thời gian check-in:</strong> {checkInTime:dd/MM/yyyy HH:mm:ss}</p>
                            <p><strong>Trạng thái:</strong> Đã tham gia</p>
                        </div>
                        
                        <p>Chúc bạn có một sự kiện tuyệt vời!</p>
                        
                        <div style='text-align: center; margin-top: 30px; font-size: 12px; color: #666;'>
                            <p>Email này được gửi tự động từ hệ thống GiaNguyenCheck</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(to, subject, body, true);
        }

        /// <summary>
        /// Gửi email bulk với template
        /// </summary>
        public async Task<List<bool>> SendBulkEmailAsync(List<string> recipients, string subject, string bodyTemplate, Dictionary<string, string>? placeholders = null)
        {
            var results = new List<bool>();

            foreach (var recipient in recipients)
            {
                var personalizedBody = bodyTemplate;
                
                // Replace placeholders if provided
                if (placeholders != null)
                {
                    foreach (var placeholder in placeholders)
                    {
                        personalizedBody = personalizedBody.Replace($"{{{placeholder.Key}}}", placeholder.Value);
                    }
                }

                var success = await SendEmailAsync(recipient, subject, personalizedBody, true);
                results.Add(success);

                // Small delay to avoid overwhelming SMTP server
                await Task.Delay(100);
            }

            return results;
        }

        /// <summary>
        /// Dispose SMTP client
        /// </summary>
        public void Dispose()
        {
            _smtpClient?.Dispose();
        }

        private string GenerateInvitationEmailTemplate(Invitation invitation, string tenantName)
        {
            var appUrl = _configuration["AppUrl"];
            var registerUrl = $"{appUrl}/auth/register?code={invitation.Code}";

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Mời tham gia {tenantName}</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .button {{ display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Mời tham gia {tenantName}</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào,</p>
                            <p>Bạn đã được mời tham gia hệ thống quản lý sự kiện <strong>{tenantName}</strong>.</p>
                            <p>Vui lòng sử dụng mã mời sau để đăng ký tài khoản:</p>
                            <h2 style='text-align: center; color: #007bff; font-size: 24px;'>{invitation.Code}</h2>
                            <p style='text-align: center;'>
                                <a href='{registerUrl}' class='button'>Đăng ký ngay</a>
                            </p>
                            <p><strong>Lưu ý:</strong></p>
                            <ul>
                                <li>Mã mời này chỉ có hiệu lực trong {invitation.ExpiresAt:dd/MM/yyyy HH:mm}</li>
                                <li>Mỗi mã mời chỉ có thể sử dụng một lần</li>
                                <li>Nếu bạn không thực hiện đăng ký, mã mời sẽ tự động hết hạn</li>
                            </ul>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động từ hệ thống {tenantName}</p>
                            <p>Vui lòng không trả lời email này</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateEventReminderTemplate(Guest guest, Event eventEntity)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Nhắc nhở sự kiện</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .event-info {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Nhắc nhở sự kiện</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{guest.FullName}</strong>,</p>
                            <p>Sự kiện <strong>{eventEntity.Name}</strong> sẽ diễn ra trong thời gian sắp tới.</p>
                            
                            <div class='event-info'>
                                <h3>Thông tin sự kiện:</h3>
                                <p><strong>Tên sự kiện:</strong> {eventEntity.Name}</p>
                                <p><strong>Thời gian:</strong> {eventEntity.StartTime:dd/MM/yyyy HH:mm} - {eventEntity.EndTime:dd/MM/yyyy HH:mm}</p>
                                <p><strong>Địa điểm:</strong> {eventEntity.Location}</p>
                                <p><strong>Mô tả:</strong> {eventEntity.Description}</p>
                            </div>
                            
                            <p>Vui lòng đảm bảo bạn có mặt đúng giờ và mang theo thông tin cá nhân để check-in.</p>
                            <p>Trân trọng,<br>Ban tổ chức</p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động từ hệ thống quản lý sự kiện</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateWelcomeEmailTemplate(User user, string tenantName)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Chào mừng</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Chào mừng đến với {tenantName}</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{user.FullName}</strong>,</p>
                            <p>Chào mừng bạn đến với hệ thống quản lý sự kiện <strong>{tenantName}</strong>!</p>
                            <p>Tài khoản của bạn đã được tạo thành công với:</p>
                            <ul>
                                <li><strong>Email:</strong> {user.Email}</li>
                                <li><strong>Vai trò:</strong> {user.Role}</li>
                            </ul>
                            <p>Bạn có thể bắt đầu sử dụng hệ thống ngay bây giờ để:</p>
                            <ul>
                                <li>Tạo và quản lý sự kiện</li>
                                <li>Quản lý danh sách khách mời</li>
                                <li>Theo dõi check-in real-time</li>
                                <li>Xem báo cáo và thống kê</li>
                            </ul>
                            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với quản trị viên hệ thống.</p>
                            <p>Trân trọng,<br>Đội ngũ {tenantName}</p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động từ hệ thống {tenantName}</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePasswordResetTemplate(User user, string resetUrl)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Đặt lại mật khẩu</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .button {{ display: inline-block; padding: 12px 24px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Đặt lại mật khẩu</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{user.FullName}</strong>,</p>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p style='text-align: center;'>
                                <a href='{resetUrl}' class='button'>Đặt lại mật khẩu</a>
                            </p>
                            <p><strong>Lưu ý:</strong></p>
                            <ul>
                                <li>Link này chỉ có hiệu lực trong 1 giờ</li>
                                <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                                <li>Để bảo mật, vui lòng không chia sẻ link này với người khác</li>
                            </ul>
                            <p>Nếu bạn gặp vấn đề, vui lòng liên hệ với quản trị viên hệ thống.</p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động từ hệ thống quản lý sự kiện</p>
                            <p>Vui lòng không trả lời email này</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePaymentConfirmationTemplate(EventPayment payment, Guest guest, Event eventEntity)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Xác nhận thanh toán</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .payment-info {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Xác nhận thanh toán</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{guest.FullName}</strong>,</p>
                            <p>Chúng tôi xác nhận đã nhận được thanh toán của bạn cho sự kiện <strong>{eventEntity.Name}</strong>.</p>
                            
                            <div class='payment-info'>
                                <h3>Thông tin thanh toán:</h3>
                                <p><strong>Mã thanh toán:</strong> {payment.Id}</p>
                                <p><strong>Số tiền:</strong> {payment.Amount:N0} {payment.Currency}</p>
                                <p><strong>Phương thức:</strong> {payment.PaymentMethod}</p>
                                <p><strong>Trạng thái:</strong> {payment.Status}</p>
                                <p><strong>Thời gian:</strong> {payment.PaidAt:dd/MM/yyyy HH:mm}</p>
                            </div>
                            
                            <div class='payment-info'>
                                <h3>Thông tin sự kiện:</h3>
                                <p><strong>Tên sự kiện:</strong> {eventEntity.Name}</p>
                                <p><strong>Thời gian:</strong> {eventEntity.StartTime:dd/MM/yyyy HH:mm} - {eventEntity.EndTime:dd/MM/yyyy HH:mm}</p>
                                <p><strong>Địa điểm:</strong> {eventEntity.Location}</p>
                            </div>
                            
                            <p>Cảm ơn bạn đã tham gia sự kiện của chúng tôi!</p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động từ hệ thống quản lý sự kiện</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateCheckInConfirmationTemplate(CheckIn checkIn, Guest guest, Event eventEntity)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Xác nhận Check-in</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .checkin-info {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Xác nhận Check-in thành công</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{guest.FullName}</strong>,</p>
                            <p>Bạn đã check-in thành công vào sự kiện <strong>{eventEntity.Name}</strong>.</p>
                            
                            <div class='checkin-info'>
                                <h3>Thông tin Check-in:</h3>
                                <p><strong>Thời gian check-in:</strong> {checkIn.CheckInTime:dd/MM/yyyy HH:mm:ss}</p>
                                <p><strong>Phương thức:</strong> {checkIn.Type}</p>
                                <p><strong>Người thực hiện:</strong> {checkIn.CheckedInByUserId}</p>
                            </div>
                            
                            <div class='checkin-info'>
                                <h3>Thông tin sự kiện:</h3>
                                <p><strong>Tên sự kiện:</strong> {eventEntity.Name}</p>
                                <p><strong>Thời gian:</strong> {eventEntity.StartTime:dd/MM/yyyy HH:mm} - {eventEntity.EndTime:dd/MM/yyyy HH:mm}</p>
                                <p><strong>Địa điểm:</strong> {eventEntity.Location}</p>
                            </div>
                            
                            <p>Chúc bạn có một sự kiện tuyệt vời!</p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động từ hệ thống quản lý sự kiện</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public async Task<bool> SendPaymentConfirmationEmailAsync(EventPayment payment, Guest guest, Event eventEntity)
        {
            var subject = $"Xác nhận thanh toán - Sự kiện {eventEntity.Name}";
            var body = GeneratePaymentConfirmationTemplate(payment, guest, eventEntity);
            
            return await SendEmailAsync(guest.Email, subject, body);
        }

        public async Task<bool> SendCheckInConfirmationEmailAsync(CheckIn checkIn, Guest guest, Event eventEntity)
        {
            var subject = $"Xác nhận Check-in - Sự kiện {eventEntity.Name}";
            var body = GenerateCheckInConfirmationTemplate(checkIn, guest, eventEntity);
            
            return await SendEmailAsync(guest.Email, subject, body);
        }
    }
} 