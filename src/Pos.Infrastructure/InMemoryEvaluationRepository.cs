using Pos.Domain;

namespace Pos.Infrastructure;

/// <summary>
/// Адаптер порта IEvaluationRepository: хранение в памяти.
/// </summary>
public class InMemoryEvaluationRepository : IEvaluationRepository
{
    private readonly Dictionary<string, Evaluation> _items = new(StringComparer.OrdinalIgnoreCase);

    public void Add(Evaluation evaluation) => _items[evaluation.Id] = evaluation;

    public void Update(Evaluation evaluation) => _items[evaluation.Id] = evaluation;

    public bool Remove(string id) => _items.Remove(id);

    public Evaluation? GetById(string id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<Evaluation> GetByStudentId(string studentId) =>
        _items.Values
            .Where(e => string.Equals(e.StudentId, studentId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public IReadOnlyList<Evaluation> GetAll() =>
        _items.Values.OrderBy(e => e.Id, StringComparer.OrdinalIgnoreCase).ToList();
}
