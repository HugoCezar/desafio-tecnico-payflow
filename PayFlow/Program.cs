using PayFlow.Models;
using PayFlow.Providers;
using PayFlow.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<FastPayProvider>();
builder.Services.AddSingleton<SecurePayProvider>();
builder.Services.AddSingleton<IPaymentProvider>(sp => sp.GetRequiredService<FastPayProvider>());
builder.Services.AddSingleton<IPaymentProvider>(sp => sp.GetRequiredService<SecurePayProvider>());
builder.Services.AddSingleton<IPaymentService, PaymentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/payments", async (PaymentRequest request, IPaymentService paymentService, CancellationToken cancellationToken) =>
    {
        try
        {
            var result = await paymentService.ProcessAsync(request, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    })
    .WithName("CreatePayment")
    .WithOpenApi();

app.Run();
