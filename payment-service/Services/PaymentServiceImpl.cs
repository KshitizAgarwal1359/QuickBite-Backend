using QuickBite.Payment.DTOs;
using QuickBite.Payment.Entities;
using QuickBite.Payment.Interfaces;
using Razorpay.Api;

namespace QuickBite.Payment.Services
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IWalletService _walletService;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentServiceImpl> _logger;

        public PaymentServiceImpl(IPaymentRepository paymentRepo, IWalletService walletService, IConfiguration config, ILogger<PaymentServiceImpl> logger)
        {
            _paymentRepo = paymentRepo;
            _walletService = walletService;
            _config = config;
            _logger = logger;
        }

        public async Task<PaymentResponseDto> ProcessPaymentAsync(int customerId, ProcessPaymentRequestDto request)
        {
            // Check if payment already exists for this order
            var existingPayment = await _paymentRepo.GetByOrderIdAsync(request.OrderId);
            if (existingPayment != null)
            {
                throw new InvalidOperationException($"Payment for Order {request.OrderId} already exists.");
            }

            var payment = new Entities.Payment
            {
                OrderId = request.OrderId,
                CustomerId = customerId,
                Amount = request.Amount,
                Mode = request.Mode,
                Currency = "INR"
            };

            if (request.Mode == "COD")
            {
                payment.Status = "PENDING";
                _logger.LogInformation("COD Payment registered for Order {OrderId}", request.OrderId);
            }
            else if (request.Mode == "WALLET")
            {
                // Delegate to wallet service
                await _walletService.PayFromWalletAsync(customerId, new WalletPayRequestDto { OrderId = request.OrderId, Amount = request.Amount });
                payment.Status = "PAID";
                payment.PaidAt = DateTime.UtcNow;
                _logger.LogInformation("WALLET Payment successful for Order {OrderId}", request.OrderId);
            }
            else if (request.Mode == "CARD" || request.Mode == "UPI")
            {
                // Verify Razorpay signature
                if (string.IsNullOrEmpty(request.RazorpayPaymentId) || string.IsNullOrEmpty(request.RazorpayOrderId) || string.IsNullOrEmpty(request.RazorpaySignature))
                {
                    throw new ArgumentException("Razorpay credentials are required for CARD/UPI payments.");
                }

                VerifyRazorpaySignature(request.RazorpayPaymentId, request.RazorpayOrderId, request.RazorpaySignature);
                
                payment.Status = "PAID";
                payment.TransactionId = request.RazorpayPaymentId;
                payment.PaidAt = DateTime.UtcNow;
                _logger.LogInformation("Razorpay {Mode} Payment verified for Order {OrderId}, Tx: {TxId}", request.Mode, request.OrderId, payment.TransactionId);
            }

            await _paymentRepo.AddAsync(payment);
            return MapToResponse(payment);
        }

        public async Task<PaymentResponseDto> GetByOrderAsync(int orderId, int customerId, string role)
        {
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null) throw new KeyNotFoundException($"Payment for Order {orderId} not found");

            if (role == "CUSTOMER" && payment.CustomerId != customerId)
                throw new UnauthorizedAccessException("You can only view your own payments.");

            return MapToResponse(payment);
        }

        public async Task<List<PaymentResponseDto>> GetByCustomerAsync(int customerId)
        {
            var payments = await _paymentRepo.GetByCustomerIdAsync(customerId);
            return payments.Select(MapToResponse).ToList();
        }

        public async Task<List<PaymentResponseDto>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepo.GetAllAsync();
            return payments.Select(MapToResponse).ToList();
        }

        public async Task<PaymentResponseDto> RefundPaymentAsync(int paymentId)
        {
            var payment = await _paymentRepo.GetByPaymentIdAsync(paymentId);
            if (payment == null) throw new KeyNotFoundException($"Payment {paymentId} not found");

            if (payment.Status != "PAID")
                throw new InvalidOperationException($"Cannot refund payment with status {payment.Status}");

            if (payment.Mode == "WALLET")
            {
                // Add money back to wallet
                await _walletService.AddToWalletAsync(payment.CustomerId, new WalletTopupRequestDto 
                { 
                    Amount = payment.Amount, 
                    RazorpayPaymentId = $"REFUND-ORDER-{payment.OrderId}" // internal ref
                });
            }
            else if (payment.Mode == "CARD" || payment.Mode == "UPI")
            {
                // Initiate Razorpay Refund
                try 
                {
                    var client = new RazorpayClient(_config["Razorpay:Key"], _config["Razorpay:Secret"]);
                    var attributes = new Dictionary<string, object>
                    {
                        {"amount", payment.Amount * 100} // Razorpay works in paise
                    };
                    
                    var refund = client.Payment.Fetch(payment.TransactionId).Refund(attributes);
                    var refundId = (string)refund["id"];
                    _logger.LogInformation("Razorpay refund successful for Tx {TxId}. RefundId: {RefId}", payment.TransactionId, refundId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Razorpay refund failed for Tx {TxId}", payment.TransactionId);
                    throw new InvalidOperationException("Failed to process refund with Payment Gateway.");
                }
            }

            payment.Status = "REFUNDED";
            payment.RefundedAt = DateTime.UtcNow;
            await _paymentRepo.UpdateAsync(payment);

            _logger.LogWarning("Payment {PaymentId} REFUNDED successfully.", paymentId);
            return MapToResponse(payment);
        }

        private void VerifyRazorpaySignature(string paymentId, string orderId, string signature)
        {
            try 
            {
                Utils.verifyPaymentSignature(new Dictionary<string, string>
                {
                    { "razorpay_payment_id", paymentId },
                    { "razorpay_order_id", orderId },
                    { "razorpay_signature", signature }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Razorpay signature verification failed.");
                throw new InvalidOperationException("Invalid payment signature.");
            }
        }

        public Task<string> CreateRazorpayOrderAsync(double amount)
        {
            try 
            {
                var client = new RazorpayClient(_config["Razorpay:Key"], _config["Razorpay:Secret"]);
                var options = new Dictionary<string, object>
                {
                    { "amount", amount * 100 }, // in paise
                    { "currency", "INR" },
                    { "receipt", Guid.NewGuid().ToString() }
                };
                var order = client.Order.Create(options);
                return Task.FromResult(order["id"].ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Razorpay order.");
                throw new InvalidOperationException("Could not initiate payment gateway.");
            }
        }

        private static PaymentResponseDto MapToResponse(Entities.Payment payment)
        {
            return new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                CustomerId = payment.CustomerId,
                Amount = payment.Amount,
                Status = payment.Status,
                Mode = payment.Mode,
                TransactionId = payment.TransactionId,
                Currency = payment.Currency,
                PaidAt = payment.PaidAt,
                RefundedAt = payment.RefundedAt
            };
        }
    }
}
