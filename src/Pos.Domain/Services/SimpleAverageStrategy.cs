namespace Pos.Domain;

/// <summary>
/// Простое среднее по рубрикатору: веса не учитываются, все критерии равнозначны.
/// </summary>
public class SimpleAverageStrategy : IGradeCalculationStrategy
{
    public string Title => "Простое среднее (без весов)";

    public decimal ComputeRawScore(IReadOnlyCollection<Criterion> rubric, IReadOnlyCollection<Evaluation> evaluations)
    {
        if (rubric.Count == 0)
            return 0m;

        decimal sumOfShares = 0m;
        foreach (var criterion in rubric)
        {
            var evaluation = evaluations.FirstOrDefault(e =>
                string.Equals(e.CriterionId, criterion.Id, StringComparison.OrdinalIgnoreCase));
            if (evaluation is not null)
                sumOfShares += criterion.Share(evaluation.Score);
        }

        return sumOfShares / rubric.Count * FinalGrade.MaxScore;
    }
}
