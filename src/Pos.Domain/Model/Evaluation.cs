namespace Pos.Domain;

/// <summary>
/// Entity: оценка — факт «этому студенту по этому критерию поставили такой балл».
/// Конструктор internal: создать оценку можно только через EvaluationFactory.
/// На другие агрегаты (студент, критерий) ссылается только по Id.
/// </summary>
public class Evaluation : Entity
{
    internal Evaluation(string id, Student student, Criterion criterion, Score score, string comment)
        : base(id, "оценки")
    {
        StudentId = student.Id;
        CriterionId = criterion.Id;
        Correct(score, criterion, comment);
    }

    public string StudentId { get; }

    public string CriterionId { get; }

    public Score Score { get; private set; }

    public string Comment { get; private set; } = string.Empty;

    public DateTime Date { get; private set; }

    /// <summary>
    /// Исправление оценки (например, после апелляции). Границы критерия проверяются снова.
    /// </summary>
    public void Correct(Score newScore, Criterion criterion, string? comment = null)
    {
        if (!string.Equals(criterion.Id, CriterionId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Оценка относится к другому критерию.");

        criterion.EnsureAllows(newScore);

        Score = newScore;
        Comment = comment?.Trim() ?? string.Empty;
        Date = DateTime.UtcNow;
    }
}
