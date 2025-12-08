using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Payments.ProcessPayment
{
    public interface IProcessPaymentUseCase : ICommandHandler<ProcessPaymentRequest, ProcessPaymentResponse> { }
}
