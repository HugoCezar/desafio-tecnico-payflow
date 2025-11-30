namespace PayFlow.Models;

public record PaymentRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";
}
