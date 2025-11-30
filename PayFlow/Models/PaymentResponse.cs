namespace PayFlow.Models;

public record PaymentResponse
{
    public int Id { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public decimal GrossAmount { get; init; }
    public decimal Fee { get; init; }
    public decimal NetAmount { get; init; }
}
