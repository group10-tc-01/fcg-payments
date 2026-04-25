using FCG.Payments.Application.Abstractions.Reports;
using FCG.Payments.Infrastructure.Pdf.Reports;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.Pdf.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructurePdf(this IServiceCollection services)
        {
            services.AddScoped<IPaymentReportPdfGenerator, PaymentReportPdfGenerator>();
            return services;
        }
    }
}
