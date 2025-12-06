using FluentValidation;

namespace FCG.Payments.Application.UseCases.Wallets.Deposit
{
    public class DepositCommandValidator : AbstractValidator<DepositCommand>
    {
        public DepositCommandValidator()
        {
            RuleFor(x => x.WalletId)
                .NotEmpty()
                .WithMessage("Wallet ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero");
        }
    }
}
