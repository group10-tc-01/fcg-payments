using FluentValidation;

namespace FCG.Payments.Application.UseCases.Wallets.DepositBalance
{
    public sealed class DepositBalanceRequestValidator : AbstractValidator<DepositBalanceRequest>
    {
        public DepositBalanceRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero");
        }
    }
}
