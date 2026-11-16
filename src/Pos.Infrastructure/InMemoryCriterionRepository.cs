using Pos.Domain;

namespace Pos.Infrastructure;

/// <summary>
/// Адаптер порта ICriterionRepository: хранение в памяти.
/// </summary>
public class InMemoryCriterionRepository : ICriterionRepository
{
    private readonly Dictionary<string, Criterion> _items = new(StringComparer.OrdinalIgnoreCase);

    public void Add(Criterion criterion) => _items[criterion.Id] = criterion;

    public Criterion? GetById(string id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<Criterion> GetAll() =>
        _items.Values.OrderBy(c => c.Id, StringComparer.OrdinalIgnoreCase).ToList();
}
