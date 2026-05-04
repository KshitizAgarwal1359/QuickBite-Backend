namespace QuickBite.Notification.Interfaces
{
    public interface INotificationService
    {
        Task Send(Entities.Notification notification);
        Task SendBulk(List<int> recipientIds, string title, string message);
        Task MarkAsRead(int notificationId);
        Task MarkAllRead(int recipientId);
        Task<IEnumerable<Entities.Notification>> GetByRecipient(int recipientId);
        Task<int> GetUnreadCount(int recipientId);
        Task DeleteNotification(int notificationId);
        Task SendEmail(string toEmail, string subject, string body);
        Task SendSms(string toPhoneNumber, string message);
        Task<IEnumerable<Entities.Notification>> GetAll();
    }
}
