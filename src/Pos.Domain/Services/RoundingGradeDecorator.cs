namespace Pos.Domain;

/// <summary>
/// Паттерн Decorator (структурный): добавляет к итогу округление до целого,
/// не трогая сам расчёт и не размазывая округление по консоли и экспортёрам.
/// Округляем неокруглённый итог, поэтому двойного округления нет.
/// </summary>
public class RoundingGradeDecorator : IGradeCalculator
{
    private readonly IGradeCalculator _inner;

    public RoundingGradeDecorator(IGradeCalculator inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public FinalGrade Calculate(
        Student student,
        IReadOnlyCollection<Criterion> rubric,
        IReadOnlyCollection<Evaluation> evaluations,
        IGradeCalculationStrategy strategy)
    {
        var grade = _inner.Calculate(student, rubric, evaluations, strategy);
        return grade.WithFinalScore(Math.Round(grade.RawScore, 0, MidpointRounding.AwayFromZero));
    }
}
