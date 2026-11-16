namespace Pos.Domain;

/// <summary>
/// Паттерн Factory (порождающий): единственное место, где создаётся Evaluation.
/// Проверяет, что критерий есть в рубрикаторе, что по нему ещё нет оценки у студента
/// и что балл не выходит за пределы критерия.
/// </summary>
public class EvaluationFactory
{
    public Evaluation Create(
        string id,
        Student student,
        IReadOnlyCollection<Criterion> rubric,
        string criterionId,
        Score score,
        IReadOnlyCollection<Evaluation> studentEvaluations,
        string comment = "")
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(studentEvaluations);

        var criterion = rubric.FirstOrDefault(c =>
                string.Equals(c.Id, criterionId?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new DomainException($"Критерия «{criterionId}» нет в рубрикаторе — оценить по нему нельзя.");

        bool alreadyEvaluated = studentEvaluations.Any(e =>
            string.Equals(e.StudentId, student.Id, StringComparison.OrdinalIgnoreCase)
            && string.Equals(e.CriterionId, criterion.Id, StringComparison.OrdinalIgnoreCase));

        if (alreadyEvaluated)
        {
            throw new DomainException(
                $"У студента «{student.FullName}» уже есть оценка по критерию «{criterion.Title}». Её можно исправить.");
        }

        criterion.EnsureAllows(score);

        return new Evaluation(id, student, criterion, score, comment);
    }
}
