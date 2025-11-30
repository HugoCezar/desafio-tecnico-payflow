namespace PayFlow.Providers;

public record ProviderChargeResult(string ExternalId, string Status, string StatusDetail, decimal Fee);
