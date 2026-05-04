using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.SignalR;
using MimeKit;
using MimeKit.Text;
using QuickBite.Notification.Hubs;
using QuickBite.Notification.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace QuickBite.Notification.Services
{
    public class NotificationServiceImpl : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationServiceImpl> _logger;

        public NotificationServiceImpl(
            INotificationRepository repository,
            IHubContext<NotificationHub> hubContext,
            IConfiguration configuration,
            ILogger<NotificationServiceImpl> logger)
        {
            _repository = repository;
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
            
            // Initialize Twilio
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
            {
                TwilioClient.Init(accountSid, authToken);
            }
        }

        public async Task Send(Entities.Notification notification)
        {
            notification.SentAt = DateTime.UtcNow;
            await _repository.AddAsync(notification);

            // SignalR in-app alert
            if (notification.Channel == "APP" || notification.Channel == "ALL")
            {
                await _hubContext.Clients.Group($"User_{notification.RecipientId}")
                    .SendAsync("ReceiveNotification", notification);
                
                // For restaurant owners hearing new-order audio ping
                if (notification.Type == "ORDER")
                {
                    await _hubContext.Clients.Group($"User_{notification.RecipientId}")
                        .SendAsync("PlayOrderAudioPing");
                }
            }

            // If we actually had the user's phone or email here we would send it
            // Assuming those methods are called separately or we fetch user info
        }

        public async Task SendBulk(List<int> recipientIds, string title, string message)
        {
            var notifications = new List<Entities.Notification>();
            foreach (var id in recipientIds)
            {
                var n = new Entities.Notification
                {
                    RecipientId = id,
                    Title = title,
                    Message = message,
                    Type = "PROMO",
                    Channel = "APP",
                    SentAt = DateTime.UtcNow
                };
                notifications.Add(n);
                await _repository.AddAsync(n);
                
                await _hubContext.Clients.Group($"User_{id}")
                    .SendAsync("ReceiveNotification", n);
            }
            _logger.LogInformation("Sent bulk notification to {Count} users", recipientIds.Count);
        }

        public async Task MarkAsRead(int notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _repository.UpdateAsync(notification);
            }
        }

        public async Task MarkAllRead(int recipientId)
        {
            var unread = await _repository.FindByRecipientIdAndIsReadAsync(recipientId, false);
            foreach (var n in unread)
            {
                n.IsRead = true;
                await _repository.UpdateAsync(n);
            }
        }

        public async Task<IEnumerable<Entities.Notification>> GetByRecipient(int recipientId)
        {
            return await _repository.FindByRecipientIdAsync(recipientId);
        }

        public async Task<int> GetUnreadCount(int recipientId)
        {
            return await _repository.CountByRecipientIdAndIsReadAsync(recipientId, false);
        }

        public async Task DeleteNotification(int notificationId)
        {
            await _repository.DeleteByNotificationIdAsync(notificationId);
        }

        public async Task SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration["Smtp:From"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            }
        }

        public async Task SendSms(string toPhoneNumber, string message)
        {
            try
            {
                var fromPhone = _configuration["Twilio:FromPhoneNumber"];
                var messageOptions = new CreateMessageOptions(new Twilio.Types.PhoneNumber(toPhoneNumber))
                {
                    From = new Twilio.Types.PhoneNumber(fromPhone),
                    Body = message
                };

                await MessageResource.CreateAsync(messageOptions);
                _logger.LogInformation("SMS sent successfully to {ToPhoneNumber}", toPhoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {ToPhoneNumber}", toPhoneNumber);
            }
        }

        public async Task<IEnumerable<Entities.Notification>> GetAll()
        {
            return await _repository.GetAllAsync();
        }
    }
}
