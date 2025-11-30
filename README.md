# PayFlow – Camada de Pagamentos (Desafio Técnico)

Camada de pagamentos desenvolvida em **.NET 9**, utilizando dois provedores de cobrança com **fallback automático**, regras de roteamento por valor e simulação completa de cálculo de taxas.

---

## 🚀 Objetivo

O projeto expõe um endpoint único `/payments` que:

1. Seleciona automaticamente o provedor mais adequado (FastPay ou SecurePay)  
2. Calcula taxa, valor líquido e gera um identificador externo  
3. Tenta automaticamente um **fallback** caso o provedor preferido esteja indisponível  
4. Retorna uma resposta padronizada independentemente do provedor utilizado  

---

## 🏗 Arquitetura

### **PaymentService**
- Aplica a regra de roteamento:
  - Valores **< R$100,00** → FastPay  
  - Valores **>= R$100,00** → SecurePay  
- Em caso de falha, alterna automaticamente para o outro provedor  
- Gera o `id` interno incremental

### **Provedores**
| Provedor | Taxa | Observações |
|---------|------|-------------|
| **FastPay** | 3,49% | Arredondado para cima em centavos |
| **SecurePay** | 2,99% + R$0,40 | Arredondado para cima em centavos |

Cada provedor implementa:
- Montagem do payload específico  
- Simulação de resposta externa  
- Cálculo da taxa  
- Geração de identificador externo

### **Interfaces e exceções**
- `IPaymentProvider`: contrato único para qualquer provider  
- `PaymentProviderUnavailableException`: indica indisponibilidade do provedor  

### **Configuração dos provedores**
A disponibilidade de cada provedor é configurável via `appsettings.json`:

```json
"Providers": {
  "FastPay": { "Enabled": true },
  "SecurePay": { "Enabled": true }
}


## Executar localmente
Pré-requisitos

.NET 9 SDK
(Opcional) Docker + Docker Compose

Rodando com .NET CLI
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
## Exemplo de Requisição
```http
POST /payments
Content-Type: application/json

{
  "amount": 120.50,
  "currency": "BRL"
}
```

Exemplo de Resposta (SecurePay)
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
Fallback Automático

Se o provedor preferido estiver indisponível:
PaymentService captura a exceção
Tenta automaticamente o outro provedor
A resposta permanece no mesmo formato, independentemente da origem

Exemplo para desativar FastPay:
Defina algum provedor como indisponivel em `appsettings.json`. O serviço tentara o provedor restante de forma transparente.

"Providers": {
  "FastPay": { "Enabled": false },
  "SecurePay": { "Enabled": true }
}


## Teste rapido via arquivo .http
O arquivo PayFlow/PayFlow.http contém requisições prontas para uso com:

Visual Studio
VS Code (REST Client)
JetBrains Rider

Basta abrir o arquivo e clicar em Send Request.

## Estrutura Simplificada
PayFlow/
 ├── Exceptions/
 ├── Models/
 ├── Providers/
 ├── Services/
 ├── appsettings.json
 ├── Program.cs

## Licença
Projeto desenvolvido exclusivamente para fins de demonstração técnica.