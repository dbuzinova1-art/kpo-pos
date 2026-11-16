using Pos.Domain;

namespace Pos.Infrastructure;

/// <summary>
/// Адаптер порта IStudentRepository: хранение в памяти.
/// </summary>
public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<string, Student> _items = new(StringComparer.OrdinalIgnoreCase);

    public void Add(Student student) => _items[student.Id] = student;

    public Student? GetById(string id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<Student> GetAll() =>
        _items.Values.OrderBy(s => s.Id, StringComparer.OrdinalIgnoreCase).ToList();
}
