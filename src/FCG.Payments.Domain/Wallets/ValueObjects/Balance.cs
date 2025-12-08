using FCG.Payments.Domain.Exceptions;
using System.Globalization;

namespace FCG.Payments.Domain.Wallets.ValueObjects
{
    public record Balance
    {
        public decimal Value { get; }

        private Balance(decimal value)
        {
            Value = value;
        }

        public static Balance Create(decimal value)
        {
            if (value <= 0)
                throw new DomainException("Balance cannot be negative or zero");

            return new Balance(value);
        }

        public Balance Add(decimal amount)
        {
            if (amount < 0)
                throw new DomainException("Cannot add negative amount to balance");

            return new Balance(Value + amount);
        }

        public Balance Subtract(decimal amount)
        {
            if (amount < 0)
                throw new DomainException("Cannot subtract negative amount from balance");

            var newValue = Value - amount;
            if (newValue < 0)
                throw new DomainException("Insufficient balance for this operation");

            return new Balance(newValue);
        }

        public static implicit operator decimal(Balance balance) => balance.Value;
        public static implicit operator Balance(decimal value) => Create(value);

        public override string ToString() => Value.ToString("C", new CultureInfo("pt-BR"));
    }
}