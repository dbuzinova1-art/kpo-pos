namespace Pos.Domain;

/// <summary>
/// Паттерн Strategy (поведенческий): сменяемое правило расчёта итога.
/// Возвращает итог по шкале [0, FinalGrade.MaxScore] без округления.
/// </summary>
public interface IGradeCalculationStrategy
{
    string Title { get; }

    decimal ComputeRawScore(IReadOnlyCollection<Criterion> rubric, IReadOnlyCollection<Evaluation> evaluations);
}
