using FluentValidation;

namespace FCG.Payments.Application.UseCases.Payments.ExportPaymentReport
{
    public sealed class ExportPaymentReportRequestValidator : AbstractValidator<ExportPaymentReportRequest>
    {
        public ExportPaymentReportRequestValidator()
        {
            RuleFor(x => x)
                .Must(x => x.DateFrom is null || x.DateTo is null || x.DateFrom <= x.DateTo)
                .WithMessage("DateFrom must be less than or equal to DateTo");
        }
    }
}
