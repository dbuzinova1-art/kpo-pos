namespace Pos.Domain;

/// <summary>
/// Порт хранилища студентов. Реализация — снаружи домена (Infrastructure).
/// </summary>
public interface IStudentRepository
{
    void Add(Student student);

    Student? GetById(string id);

    IReadOnlyList<Student> GetAll();
}
