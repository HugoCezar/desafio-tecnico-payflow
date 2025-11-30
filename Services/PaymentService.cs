using PayFlow.Exceptions;
using PayFlow.Models;
using PayFlow.Providers;

namespace PayFlow.Services;

public class PaymentService : IPaymentService
{
    private const decimal SecurePayThreshold = 100m;
    private static int _idSeed;

    private readonly IPaymentProvider _fastPay;
    private readonly IPaymentProvider _securePay;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(FastPayProvider fastPay, SecurePayProvider securePay, ILogger<PaymentService> logger)
    {
        _fastPay = fastPay;
        _securePay = securePay;
        _logger = logger;
    }

    public async Task<PaymentResponse> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(request.Amount));
        }

        var preferredProvider = request.Amount < SecurePayThreshold ? _fastPay : _securePay;
        var fallbackProvider = ReferenceEquals(preferredProvider, _fastPay) ? _securePay : _fastPay;

        var chargeResult = await TryChargeAsync(preferredProvider, request, cancellationToken)
                           ?? await TryChargeAsync(fallbackProvider, request, cancellationToken)
                           ?? throw new InvalidOperationException("Nenhum provedor de pagamento está disponível no momento.");

        var grossAmount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
        var netAmount = decimal.Round(grossAmount - chargeResult.Result.Fee, 2, MidpointRounding.AwayFromZero);

        return new PaymentResponse
        {
            Id = Interlocked.Increment(ref _idSeed),
            ExternalId = chargeResult.Result.ExternalId,
            Status = NormalizeStatus(chargeResult.Result.Status),
            Provider = chargeResult.Provider.Name,
            GrossAmount = grossAmount,
            Fee = chargeResult.Result.Fee,
            NetAmount = netAmount
        };
    }

    private async Task<(IPaymentProvider Provider, ProviderChargeResult Result)?> TryChargeAsync(
        IPaymentProvider provider,
        PaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!provider.IsEnabled)
        {
            _logger.LogWarning("Provider {Provider} is disabled. Skipping.", provider.Name);
            return null;
        }

        try
        {
            var result = await provider.ChargeAsync(request, cancellationToken);
            return (provider, result);
        }
        catch (PaymentProviderUnavailableException ex)
        {
            _logger.LogWarning(ex, "Provider {Provider} unavailable. Trying fallback.", provider.Name);
            return null;
        }
    }

    private static string NormalizeStatus(string providerStatus)
    {
        return providerStatus.Equals("approved", StringComparison.OrdinalIgnoreCase)
               || providerStatus.Equals("success", StringComparison.OrdinalIgnoreCase)
            ? "approved"
            : providerStatus.ToLowerInvariant();
    }
}
