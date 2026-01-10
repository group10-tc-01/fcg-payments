using FCG.Payments.Domain.Payments;

namespace FCG.Payments.Application.UseCases.Payments.ProcessPayment
{
    public record ProcessPaymentResponse(Guid CorrelationId, Guid PaymentId, Guid UserId, Guid GameId, decimal Amount, PaymentStatus Status);
}
