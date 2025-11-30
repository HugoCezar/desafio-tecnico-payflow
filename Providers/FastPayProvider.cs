using PayFlow.Exceptions;
using PayFlow.Models;

namespace PayFlow.Providers;

public class FastPayProvider : IPaymentProvider
{
    private const decimal FeePercentage = 0.0349m;
    private readonly ILogger<FastPayProvider> _logger;
    private readonly bool _enabled;

    public FastPayProvider(IConfiguration configuration, ILogger<FastPayProvider> logger)
    {
        _logger = logger;
        _enabled = configuration.GetValue<bool?>("Providers:FastPay:Enabled") ?? true;
    }

    public string Name => "FastPay";

    public bool IsEnabled => _enabled;

    public Task<ProviderChargeResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        if (!IsEnabled)
        {
            throw new PaymentProviderUnavailableException(Name, "FastPay disabled by configuration.");
        }

        var payload = new
        {
            transaction_amount = request.Amount,
            currency = request.Currency,
            payer = new { email = "cliente@teste.com" },
            installments = 1,
            description = "Compra via FastPay"
        };

        _logger.LogInformation("Sending payload to FastPay: {@Payload}", payload);

        var response = new
        {
            id = $"FP-{Random.Shared.Next(100000, 999999)}",
            status = "approved",
            status_detail = "Pagamento aprovado"
        };

        _logger.LogInformation("Received response from FastPay: {@Response}", response);

        var fee = decimal.Round(request.Amount * FeePercentage, 2, MidpointRounding.AwayFromZero);

        return Task.FromResult(new ProviderChargeResult(response.id, response.status, response.status_detail, fee));
    }
}
