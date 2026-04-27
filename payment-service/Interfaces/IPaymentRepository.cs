namespace QuickBite.Payment.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Entities.Payment?> GetByOrderIdAsync(int orderId);
        Task<Entities.Payment?> GetByPaymentIdAsync(int paymentId);
        Task<List<Entities.Payment>> GetByCustomerIdAsync(int customerId);
        Task<List<Entities.Payment>> GetAllAsync();
        Task<Entities.Payment> AddAsync(Entities.Payment payment);
        Task UpdateAsync(Entities.Payment payment);
    }
}
