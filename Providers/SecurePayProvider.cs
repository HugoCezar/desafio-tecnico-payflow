using PayFlow.Exceptions;
using PayFlow.Models;

namespace PayFlow.Providers;

public class SecurePayProvider : IPaymentProvider
{
    private const decimal FeePercentage = 0.0299m;
    private const decimal FixedFee = 0.40m;

    private readonly ILogger<SecurePayProvider> _logger;
    private readonly bool _enabled;

    public SecurePayProvider(IConfiguration configuration, ILogger<SecurePayProvider> logger)
    {
        _logger = logger;
        _enabled = configuration.GetValue<bool?>("Providers:SecurePay:Enabled") ?? true;
    }

    public string Name => "SecurePay";

    public bool IsEnabled => _enabled;

    public Task<ProviderChargeResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        if (!IsEnabled)
        {
            throw new PaymentProviderUnavailableException(Name, "SecurePay disabled by configuration.");
        }

        var payload = new
        {
            amount_cents = (int)Math.Round(request.Amount * 100, MidpointRounding.AwayFromZero),
            currency_code = request.Currency,
            client_reference = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}"
        };

        _logger.LogInformation("Sending payload to SecurePay: {@Payload}", payload);

        var response = new
        {
            transaction_id = $"SP-{Random.Shared.Next(10000, 99999)}",
            result = "success"
        };

        _logger.LogInformation("Received response from SecurePay: {@Response}", response);

        var fee = decimal.Round(request.Amount * FeePercentage + FixedFee, 2, MidpointRounding.AwayFromZero);

        return Task.FromResult(new ProviderChargeResult(response.transaction_id, response.result, "Pagamento aprovado", fee));
    }
}
