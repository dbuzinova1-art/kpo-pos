namespace Pos.Domain;

/// <summary>
/// Entity: студент — кого оценивают.
/// </summary>
public class Student : Entity
{
    public Student(string id, string fullName) : base(id, "студента")
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Имя студента не может быть пустым.");

        FullName = fullName.Trim();
    }

    public string FullName { get; }
}
