namespace Pos.Domain;

/// <summary>
/// Базовая сущность: две сущности одного типа равны, если совпадает Id
/// (у Value Object, наоборот, равенство определяется значениями).
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    protected Entity(string id, string entityName)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException($"ID {entityName} не может быть пустым.");

        Id = id.Trim();
    }

    public string Id { get; }

    public bool Equals(Entity? other) =>
        other is not null
        && GetType() == other.GetType()
        && string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
}
