using QuickBite.Payment.DTOs;

namespace QuickBite.Payment.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> ProcessPaymentAsync(int customerId, ProcessPaymentRequestDto request);
        Task<PaymentResponseDto> GetByOrderAsync(int orderId, int customerId, string role);
        Task<List<PaymentResponseDto>> GetByCustomerAsync(int customerId);
        Task<List<PaymentResponseDto>> GetAllPaymentsAsync();
        Task<PaymentResponseDto> RefundPaymentAsync(int paymentId);
        Task<string> CreateRazorpayOrderAsync(double amount);
    }
}
