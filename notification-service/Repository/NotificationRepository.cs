using Microsoft.EntityFrameworkCore;
using QuickBite.Notification.Data;
using QuickBite.Notification.Interfaces;

namespace QuickBite.Notification.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Entities.Notification> AddAsync(Entities.Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Entities.Notification>> FindByRecipientIdAsync(int recipientId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Entities.Notification>> FindByRecipientIdAndIsReadAsync(int recipientId, bool isRead)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == recipientId && n.IsRead == isRead)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<int> CountByRecipientIdAndIsReadAsync(int recipientId, bool isRead)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == recipientId && n.IsRead == isRead);
        }

        public async Task<IEnumerable<Entities.Notification>> FindByTypeAsync(string type)
        {
            return await _context.Notifications
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Entities.Notification>> FindByRelatedIdAsync(int relatedId)
        {
            return await _context.Notifications
                .Where(n => n.RelatedId == relatedId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<Entities.Notification?> GetByIdAsync(int notificationId)
        {
            return await _context.Notifications.FindAsync(notificationId);
        }

        public async Task UpdateAsync(Entities.Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByNotificationIdAsync(int notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Entities.Notification>> GetAllAsync()
        {
            return await _context.Notifications
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }
    }
}
