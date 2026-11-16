namespace Pos.Domain;

/// <summary>
/// Entity: критерий — за что ставят балл. Есть максимум и вес (вклад в итог).
/// Правило «балл не выходит за пределы критерия» знает сам критерий.
/// </summary>
public class Criterion : Entity
{
    public Criterion(string id, string title, Score maxScore, Weight weight) : base(id, "критерия")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Название критерия не может быть пустым.");
        if (maxScore.Value <= 0)
            throw new DomainException("Максимальный балл критерия должен быть больше нуля.");

        Title = title.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }

    public string Title { get; }

    public Score MaxScore { get; }

    public Weight Weight { get; }

    /// <summary>
    /// Инвариант: балл должен лежать в диапазоне [0, MaxScore].
    /// (Отрицательный балл не даст создать сам Score.)
    /// </summary>
    public void EnsureAllows(Score score)
    {
        if (score.Value > MaxScore.Value)
            throw new DomainException($"Нельзя поставить {score} из {MaxScore} по критерию «{Title}».");
    }

    /// <summary>
    /// Какая доля максимума набрана: от 0 до 1.
    /// </summary>
    public decimal Share(Score score) => score.Value / MaxScore.Value;
}
