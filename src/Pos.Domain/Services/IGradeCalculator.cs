namespace Pos.Domain;

/// <summary>
/// Контракт доменного сервиса расчёта итога. Нужен, чтобы поверх него можно было навесить Decorator.
/// </summary>
public interface IGradeCalculator
{
    FinalGrade Calculate(
        Student student,
        IReadOnlyCollection<Criterion> rubric,
        IReadOnlyCollection<Evaluation> evaluations,
        IGradeCalculationStrategy strategy);
}
