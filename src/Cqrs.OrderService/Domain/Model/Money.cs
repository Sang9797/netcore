namespace Cqrs.OrderService.Domain.Model;

public sealed class Money : IEquatable<Money>
{
    public static Money Zero { get; } = new(0, "USD");

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = string.IsNullOrWhiteSpace(currency)
            ? throw new ArgumentException("currency required", nameof(currency))
            : currency.ToUpperInvariant();
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public Money Add(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Currency mismatch: {Currency} / {other.Currency}");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("qty cannot be negative", nameof(quantity));
        }

        return new Money(Amount * quantity, Currency);
    }

    public bool Equals(Money? other) =>
        other is not null && Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
}
