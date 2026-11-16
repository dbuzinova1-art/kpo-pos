namespace Pos.Domain;

/// <summary>
/// Взвешенная сумма: каждый критерий даёт (балл / максимум) * вес, дальше всё приводится к 10-балльной шкале.
/// Если веса в сумме дают 1, это обычное «сумма(балл * вес)». Если оценки по критерию нет, он вносит 0.
/// </summary>
public class WeightedSumStrategy : IGradeCalculationStrategy
{
    public string Title => "Взвешенная сумма (баллы с учётом весов)";

    public decimal ComputeRawScore(IReadOnlyCollection<Criterion> rubric, IReadOnlyCollection<Evaluation> evaluations)
    {
        decimal totalWeight = rubric.Sum(c => c.Weight.Value);
        if (totalWeight == 0m)
            return 0m;

        decimal weightedShare = 0m;
        foreach (var criterion in rubric)
        {
            var evaluation = evaluations.FirstOrDefault(e =>
                string.Equals(e.CriterionId, criterion.Id, StringComparison.OrdinalIgnoreCase));
            if (evaluation is not null)
                weightedShare += criterion.Share(evaluation.Score) * criterion.Weight.Value;
        }

        return weightedShare / totalWeight * FinalGrade.MaxScore;
    }
}
