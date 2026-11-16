namespace Pos.Domain;

/// <summary>
/// Порт хранилища оценок.
/// </summary>
public interface IEvaluationRepository
{
    void Add(Evaluation evaluation);

    void Update(Evaluation evaluation);

    bool Remove(string id);

    Evaluation? GetById(string id);

    IReadOnlyList<Evaluation> GetByStudentId(string studentId);

    IReadOnlyList<Evaluation> GetAll();
}
