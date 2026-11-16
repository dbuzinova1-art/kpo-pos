using Pos.Domain;
using Xunit;

namespace Pos.Tests;

/// <summary>
/// Тесты правил предметной области: модель не даёт их нарушить.
/// </summary>
public class InvariantTests
{
    private static readonly EvaluationFactory Factory = new();

    private static Student Ivanov() => new("S1", "Иванов");

    private static Criterion Homework(string id = "HW-1", decimal max = 10m) =>
        new(id, "ДЗ", new Score(max), new Weight(0.5m));

    [Fact]
    public void Score_CannotBeNegative()
    {
        Assert.Throws<DomainException>(() => new Score(-1m));
    }

    [Fact]
    public void Weight_MustBePositive()
    {
        Assert.Throws<DomainException>(() => new Weight(0m));
        Assert.Throws<DomainException>(() => new Weight(-0.5m));
    }

    [Fact]
    public void Criterion_MaxScoreMustBePositive_AndTitleNotEmpty()
    {
        Assert.Throws<DomainException>(() => new Criterion("C1", "ДЗ", new Score(0m), new Weight(1m)));
        Assert.Throws<DomainException>(() => new Criterion("C1", "  ", new Score(10m), new Weight(1m)));
    }

    [Fact]
    public void Student_RequiresIdAndName()
    {
        Assert.Throws<DomainException>(() => new Student("", "Иванов"));
        Assert.Throws<DomainException>(() => new Student("S1", " "));
    }

    [Fact]
    public void Factory_RejectsScoreAboveCriterionMax()
    {
        // Из ТЗ: «Нельзя поставить 12 баллов из 10»
        var criterion = Homework(max: 10m);

        var ex = Assert.Throws<DomainException>(() =>
            Factory.Create("E1", Ivanov(), new[] { criterion }, "HW-1", new Score(12m), Array.Empty<Evaluation>()));

        Assert.Contains("12", ex.Message);
    }

    [Fact]
    public void Factory_AllowsScoreEqualToMax()
    {
        var criterion = Homework(max: 10m);

        var evaluation = Factory.Create("E1", Ivanov(), new[] { criterion }, "HW-1", new Score(10m), Array.Empty<Evaluation>());

        Assert.Equal(10m, evaluation.Score.Value);
    }

    [Fact]
    public void Factory_RejectsCriterionNotInRubric()
    {
        // Из ТЗ: «Нельзя оценить критерий, которого нет в рубрикаторе»
        var rubric = new[] { Homework("HW-1") };

        Assert.Throws<DomainException>(() =>
            Factory.Create("E1", Ivanov(), rubric, "HW-99", new Score(5m), Array.Empty<Evaluation>()));
    }

    [Fact]
    public void Factory_RejectsSecondEvaluationForSameCriterion()
    {
        var student = Ivanov();
        var rubric = new[] { Homework("HW-1") };
        var first = Factory.Create("E1", student, rubric, "HW-1", new Score(5m), Array.Empty<Evaluation>());

        Assert.Throws<DomainException>(() =>
            Factory.Create("E2", student, rubric, "HW-1", new Score(6m), new[] { first }));
    }

    [Fact]
    public void Evaluation_Correct_ChangesScore_ButKeepsCriterionBounds()
    {
        var criterion = Homework(max: 10m);
        var evaluation = Factory.Create("E1", Ivanov(), new[] { criterion }, "HW-1", new Score(6m), Array.Empty<Evaluation>());

        evaluation.Correct(new Score(9m), criterion, "После апелляции");
        Assert.Equal(9m, evaluation.Score.Value);
        Assert.Equal("После апелляции", evaluation.Comment);

        Assert.Throws<DomainException>(() => evaluation.Correct(new Score(11m), criterion));
        Assert.Equal(9m, evaluation.Score.Value); // неудачное исправление ничего не меняет
    }

    [Fact]
    public void Evaluation_CannotBeCorrectedWithForeignCriterion()
    {
        var hw1 = Homework("HW-1");
        var hw2 = Homework("HW-2");
        var evaluation = Factory.Create("E1", Ivanov(), new[] { hw1, hw2 }, "HW-1", new Score(6m), Array.Empty<Evaluation>());

        Assert.Throws<DomainException>(() => evaluation.Correct(new Score(7m), hw2));
    }

    [Fact]
    public void Entities_AreEqualById_ValueObjectsByValue()
    {
        Assert.Equal(new Student("S1", "Иванов"), new Student("s1", "Другое имя"));
        Assert.NotEqual(new Student("S1", "Иванов"), new Student("S2", "Иванов"));
        Assert.Equal(new Score(5m), new Score(5m));
    }
}
