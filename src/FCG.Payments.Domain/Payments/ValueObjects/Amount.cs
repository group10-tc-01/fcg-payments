using FCG.Payments.Domain.Exceptions;

namespace FCG.Payments.Domain.Payments.ValueObjects
{
    public sealed record Amount
    {
        public decimal Value { get; }

        private Amount(decimal value)
        {
            if (value < 0)
            {
                throw new DomainException("Amount cannot be negative or zero");
            }

            Value = value;
        }

        public static Amount Create(decimal value)
        {
            return new Amount(value);
        }

        public static implicit operator decimal(Amount price) => price.Value;
        public static implicit operator Amount(decimal value) => Create(value);

        public override string ToString() => Value.ToString("F2");
    }
}
