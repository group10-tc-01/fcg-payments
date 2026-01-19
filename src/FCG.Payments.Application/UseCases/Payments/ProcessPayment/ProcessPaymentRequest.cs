using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Payments.ProcessPayment
{
    public record ProcessPaymentRequest(string UserEmail, Guid CorrelationId, Guid UserId, Guid GameId, decimal Amount) : ICommand<ProcessPaymentResponse>;
}
