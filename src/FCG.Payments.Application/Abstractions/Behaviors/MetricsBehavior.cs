using FCG.Payments.Application.Abstractions.Messaging;
using FCG.Payments.Application.Observability;
using MediatR;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Application.Abstractions.Behaviors
{
    [ExcludeFromCodeCoverage]
    public class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICommand<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            string commandName = request.GetType().Name;
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                TResponse response = await next();

                ApplicationMetrics.RecordExecution(commandName, success: true, stopwatch.Elapsed.TotalMilliseconds);

                return response;
            }
            catch (Exception exception)
            {
                ApplicationMetrics.RecordFailure(commandName, exception.GetType().Name, stopwatch.Elapsed.TotalMilliseconds);
                throw;
            }
        }
    }
}
