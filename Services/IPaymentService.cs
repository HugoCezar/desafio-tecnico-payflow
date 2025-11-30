using PayFlow.Models;

namespace PayFlow.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken);
}
