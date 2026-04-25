using FluentValidation;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentReport
{
    public sealed class GetPaymentReportRequestValidator : AbstractValidator<GetPaymentReportRequest>
    {
        public GetPaymentReportRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("PageNumber must be greater than zero");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than zero")
                .LessThanOrEqualTo(50)
                .WithMessage("PageSize must be less than or equal to 50");

            RuleFor(x => x)
                .Must(x => x.DateFrom is null || x.DateTo is null || x.DateFrom <= x.DateTo)
                .WithMessage("DateFrom must be less than or equal to DateTo");
        }
    }
}
