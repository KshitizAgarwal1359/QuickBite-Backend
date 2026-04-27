using Microsoft.EntityFrameworkCore;
using QuickBite.Payment.Data;
using QuickBite.Payment.Interfaces;

namespace QuickBite.Payment.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context) { _context = context; }

        public async Task<Entities.Payment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Entities.Payment?> GetByPaymentIdAsync(int paymentId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        public async Task<List<Entities.Payment>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Payments
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentId)
                .ToListAsync();
        }

        public async Task<List<Entities.Payment>> GetAllAsync()
        {
            return await _context.Payments.OrderByDescending(p => p.PaymentId).ToListAsync();
        }

        public async Task<Entities.Payment> AddAsync(Entities.Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task UpdateAsync(Entities.Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
}
