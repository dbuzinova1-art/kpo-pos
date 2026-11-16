namespace Pos.Domain;

/// <summary>
/// Domain Service: расчёт итога по студенту.
/// Правило связывает сразу студента, рубрикатор и оценки, поэтому не принадлежит ни одной сущности.
/// Сервис без состояния: правило подсчёта приходит параметром (Strategy).
/// </summary>
public class GradeCalculationService : IGradeCalculator
{
    public FinalGrade Calculate(
        Student student,
        IReadOnlyCollection<Criterion> rubric,
        IReadOnlyCollection<Evaluation> evaluations,
        IGradeCalculationStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(evaluations);
        ArgumentNullException.ThrowIfNull(strategy);

        // Берём только оценки этого студента по критериям, которые есть в рубрикаторе.
        var rubricIds = rubric.Select(c => c.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var own = evaluations
            .Where(e => string.Equals(e.StudentId, student.Id, StringComparison.OrdinalIgnoreCase)
                        && rubricIds.Contains(e.CriterionId))
            .ToList();

        decimal raw = strategy.ComputeRawScore(rubric, own);
        return new FinalGrade(student.Id, raw, raw, own.Count, rubric.Count);
    }
}
