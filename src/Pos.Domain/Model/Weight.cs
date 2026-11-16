namespace Pos.Domain;

/// <summary>
/// Value Object: вес критерия (его вклад в итог).
/// Инвариант: вес строго положительный.
/// </summary>
public readonly record struct Weight
{
    public Weight(decimal value)
    {
        if (value <= 0)
            throw new DomainException($"Вес критерия должен быть больше нуля (получено: {value}).");

        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => $"{Value:0.##}";
}
