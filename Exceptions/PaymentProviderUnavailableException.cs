namespace PayFlow.Exceptions;

public class PaymentProviderUnavailableException : Exception
{
    public PaymentProviderUnavailableException(string providerName, string? message = null)
        : base(message ?? $"Provider {providerName} is unavailable.")
    {
        ProviderName = providerName;
    }

    public string ProviderName { get; }
}
