namespace Pos.Domain;

/// <summary>
/// Исключение при нарушении правил предметной области (инвариантов).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
