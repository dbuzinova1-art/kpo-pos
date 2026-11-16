namespace Pos.Domain;

/// <summary>
/// Value Object: балл. Неизменяемый, равенство по значению.
/// Инвариант: балл не может быть отрицательным.
/// </summary>
public readonly record struct Score
{
    public Score(decimal value)
    {
        if (value < 0)
            throw new DomainException($"Балл не может быть отрицательным (получено: {value}).");

        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => $"{Value:0.##}";
}
