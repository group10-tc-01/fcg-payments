using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Payments.ProcessPayment
{
    public record ProcessPaymentRequest(Guid CorrelationId, Guid UserId, Guid GameId, decimal Amount) : ICommand<ProcessPaymentResponse>;
}
