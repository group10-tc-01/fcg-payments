using FluentValidation;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentHistory
{
    public sealed class GetPaymentHistoryRequestValidator : AbstractValidator<GetPaymentHistoryRequest>
    {
        public GetPaymentHistoryRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("PageNumber must be greater than zero");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than zero")
                .LessThanOrEqualTo(50)
                .WithMessage("PageSize must be less than or equal to 50");

            RuleFor(x => x.DateFrom)
                .LessThanOrEqualTo(x => x.DateTo)
                .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
                .WithMessage("DateFrom must be less than or equal to DateTo");
        }
    }
}
