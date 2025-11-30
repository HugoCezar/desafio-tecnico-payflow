using PayFlow.Models;

namespace PayFlow.Providers;

public interface IPaymentProvider
{
    string Name { get; }
    bool IsEnabled { get; }
    Task<ProviderChargeResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken);
}
