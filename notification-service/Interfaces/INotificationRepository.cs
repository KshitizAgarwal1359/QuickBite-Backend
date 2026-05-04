namespace QuickBite.Notification.Interfaces
{
    public interface INotificationRepository
    {
        Task<Entities.Notification> AddAsync(Entities.Notification notification);
        Task<IEnumerable<Entities.Notification>> FindByRecipientIdAsync(int recipientId);
        Task<IEnumerable<Entities.Notification>> FindByRecipientIdAndIsReadAsync(int recipientId, bool isRead);
        Task<int> CountByRecipientIdAndIsReadAsync(int recipientId, bool isRead);
        Task<IEnumerable<Entities.Notification>> FindByTypeAsync(string type);
        Task<IEnumerable<Entities.Notification>> FindByRelatedIdAsync(int relatedId);
        Task<Entities.Notification?> GetByIdAsync(int notificationId);
        Task UpdateAsync(Entities.Notification notification);
        Task DeleteByNotificationIdAsync(int notificationId);
        Task<IEnumerable<Entities.Notification>> GetAllAsync();
    }
}
