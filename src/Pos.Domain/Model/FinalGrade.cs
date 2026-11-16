namespace Pos.Domain;

/// <summary>
/// Value Object: итог по студенту. Не хранится, а считается из оценок и рубрикатора.
/// Инвариант: итог лежит в пределах шкалы [0, MaxScore].
/// </summary>
public readonly record struct FinalGrade
{
    /// <summary>Итоговая шкала — 10 баллов.</summary>
    public const decimal MaxScore = 10m;

    public FinalGrade(string studentId, decimal rawScore, decimal finalScore, int evaluatedCount, int totalCriteriaCount)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new DomainException("ID студента не может быть пустым.");
        if (rawScore < 0 || rawScore > MaxScore || finalScore < 0 || finalScore > MaxScore)
            throw new DomainException($"Итог должен быть в пределах от 0 до {MaxScore}.");

        StudentId = studentId.Trim();
        RawScore = rawScore;
        FinalScore = finalScore;
        EvaluatedCount = evaluatedCount;
        TotalCriteriaCount = totalCriteriaCount;
    }

    public string StudentId { get; }

    /// <summary>Итог до округления.</summary>
    public decimal RawScore { get; }

    /// <summary>Итог, который идёт в ведомость.</summary>
    public decimal FinalScore { get; }

    public int EvaluatedCount { get; }

    public int TotalCriteriaCount { get; }

    public FinalGrade WithFinalScore(decimal finalScore) =>
        new FinalGrade(StudentId, RawScore, finalScore, EvaluatedCount, TotalCriteriaCount);
}
