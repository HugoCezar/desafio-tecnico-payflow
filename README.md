# PayFlow

Camada de pagamentos em .NET 9 com dois provedores e troca automatica/fallback.

## Arquitetura
- `PaymentService` aplica a regra de roteamento (valor < 100 usa FastPay, caso contrario SecurePay) e tenta fallback se o provedor preferido estiver indisponivel.
- `FastPayProvider` e `SecurePayProvider` montam o payload especifico de cada integracao, simulam a resposta esperada e calculam a taxa do provedor.
- `IPaymentProvider` define o contrato unico de cobranca; `PaymentProviderUnavailableException` sinaliza indisponibilidade.
- Taxas: FastPay 3,49% (arredondado para cima em centavos); SecurePay 2,99% + R$0,40 (arredondado para cima em centavos).
- Configuracao de disponibilidade em `appsettings*.json` (`Providers:FastPay:Enabled` e `Providers:SecurePay:Enabled`).
- A API responde sempre com valores `grossAmount`, `fee`, `netAmount`, `provider`, `status`, `externalId` e um `id` interno incremental.

## Executar localmente
```bash
dotnet restore
dotnet run --project PayFlow
```

Endpoint padrao: `http://localhost:5136/payments`

## Via Docker Compose
```bash
docker compose up --build
```

Endpoint padrao: `http://localhost:8080/payments`

## Requisicao/Resposta
```http
POST /payments
Content-Type: application/json

{
  "amount": 120.50,
  "currency": "BRL"
}
```

Resposta (exemplo para SecurePay):
```json
{
  "id": 1,
  "externalId": "SP-19283",
  "status": "approved",
  "provider": "SecurePay",
  "grossAmount": 120.50,
  "fee": 4.01,
  "netAmount": 116.49
}
```

## Fallback de provedor
Defina algum provedor como indisponivel em `appsettings.json` (por exemplo, `"Providers:FastPay:Enabled": false`). O serviço tentara o provedor restante de forma transparente.

## Teste rapido via arquivo .http
`PayFlow/PayFlow.http` ja contem a chamada POST para facilitar o teste em IDEs que suportam o formato.
