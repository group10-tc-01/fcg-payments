using FCG.Payments.Domain.Exceptions;
using System.Globalization;

namespace FCG.Payments.Domain.Wallets.ValueObjects
{
    public class Balance
    {
        public decimal Value { get; private set; }

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

        internal void ChangeValue(decimal newValue)
        {
            if (newValue < 0)
                throw new DomainException("Balance cannot be negative");

            Value = newValue;
        }

        public static implicit operator decimal(Balance balance) => balance.Value;
        public static implicit operator Balance(decimal value) => Create(value);

        public override string ToString() => Value.ToString("C", new CultureInfo("pt-BR"));
    }
}