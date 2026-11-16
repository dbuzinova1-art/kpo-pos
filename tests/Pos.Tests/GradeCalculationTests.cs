using Pos.Domain;
using Xunit;

namespace Pos.Tests;

/// <summary>
/// Тесты Domain Service (расчёт итога), стратегий и декоратора округления.
/// </summary>
public class GradeCalculationTests
{
    private static readonly EvaluationFactory Factory = new();
    private static readonly GradeCalculationService Service = new();
    private static readonly Student Ivanov = new("S1", "Иванов");

    private static Criterion NewCriterion(string id, decimal max, decimal weight) =>
        new(id, id, new Score(max), new Weight(weight));

    private static Evaluation Evaluate(List<Criterion> rubric, string criterionId, decimal score) =>
        Factory.Create("E-" + criterionId, Ivanov, rubric, criterionId, new Score(score), Array.Empty<Evaluation>());

    [Fact]
    public void WeightedSum_CalculatesByWeights()
    {
        // 0.3 * 8.5 + 0.4 * 9 + 0.3 * 9 = 2.55 + 3.6 + 2.7 = 8.85
        var rubric = new List<Criterion> { NewCriterion("C1", 10m, 0.3m), NewCriterion("C2", 10m, 0.4m), NewCriterion("C3", 10m, 0.3m) };
        var evaluations = new List<Evaluation>
        {
            Evaluate(rubric, "C1", 8.5m),
            Evaluate(rubric, "C2", 9m),
            Evaluate(rubric, "C3", 9m)
        };

        var grade = Service.Calculate(Ivanov, rubric, evaluations, new WeightedSumStrategy());

        Assert.Equal(8.85m, grade.RawScore);
        Assert.Equal(3, grade.EvaluatedCount);
        Assert.Equal(3, grade.TotalCriteriaCount);
    }

    [Fact]
    public void WeightedSum_CriterionWithoutEvaluationCountsAsZero()
    {
        var rubric = new List<Criterion> { NewCriterion("C1", 10m, 0.5m), NewCriterion("C2", 10m, 0.5m) };
        var evaluations = new List<Evaluation> { Evaluate(rubric, "C1", 10m) };

        var grade = Service.Calculate(Ivanov, rubric, evaluations, new WeightedSumStrategy());

        Assert.Equal(5m, grade.RawScore);
        Assert.Equal(1, grade.EvaluatedCount);
        Assert.Equal(2, grade.TotalCriteriaCount);
    }

    [Fact]
    public void WeightedSum_WorksWhenWeightsDoNotSumToOne()
    {
        // веса 2 и 2: пятёрка из десяти за один из двух критериев — ровно половина шкалы
        var rubric = new List<Criterion> { NewCriterion("C1", 10m, 2m), NewCriterion("C2", 10m, 2m) };
        var evaluations = new List<Evaluation> { Evaluate(rubric, "C1", 10m) };

        var grade = Service.Calculate(Ivanov, rubric, evaluations, new WeightedSumStrategy());

        Assert.Equal(5m, grade.RawScore);
    }

    [Fact]
    public void WeightedSum_TakesCriterionMaxScoreIntoAccount()
    {
        // 5 из 5 даёт долю 1, 5 из 10 даёт долю 0.5: (0.5 * 1 + 0.5 * 0.5) * 10 = 7.5
        var rubric = new List<Criterion> { NewCriterion("C1", 5m, 0.5m), NewCriterion("C2", 10m, 0.5m) };
        var evaluations = new List<Evaluation> { Evaluate(rubric, "C1", 5m), Evaluate(rubric, "C2", 5m) };

        var grade = Service.Calculate(Ivanov, rubric, evaluations, new WeightedSumStrategy());

        Assert.Equal(7.5m, grade.RawScore);
    }

    [Fact]
    public void SimpleAverage_IgnoresWeights()
    {
        var rubric = new List<Criterion> { NewCriterion("C1", 10m, 0.3m), NewCriterion("C2", 10m, 0.7m) };
        var evaluations = new List<Evaluation> { Evaluate(rubric, "C1", 6m), Evaluate(rubric, "C2", 10m) };

        var grade = Service.Calculate(Ivanov, rubric, evaluations, new SimpleAverageStrategy());

        Assert.Equal(8m, grade.RawScore);
    }

    [Fact]
    public void Strategy_ChangesResultForSameData()
    {
        var rubric = new List<Criterion> { NewCriterion("C1", 10m, 0.9m), NewCriterion("C2", 10m, 0.1m) };
        var evaluations = new List<Evaluation> { Evaluate(rubric, "C1", 10m), Evaluate(rubric, "C2", 0m) };

        var weighted = Service.Calculate(Ivanov, rubric, evaluations, new WeightedSumStrategy());
        var average = Service.Calculate(Ivanov, rubric, evaluations, new SimpleAverageStrategy());

        Assert.Equal(9m, weighted.RawScore);
        Assert.Equal(5m, average.RawScore);
    }

    [Fact]
    public void Service_IgnoresEvaluationsOfOtherStudents()
    {
        var rubric = new List<Criterion> { NewCriterion("C1", 10m, 1m) };
        var other = new Student("S2", "Петров");
        var foreign = Factory.Create("E-9", other, rubric, "C1", new Score(10m), Array.Empty<Evaluation>());

        var grade = Service.Calculate(Ivanov, rubric, new List<Evaluation> { foreign }, new WeightedSumStrategy());

        Assert.Equal(0m, grade.RawScore);
        Assert.Equal(0, grade.EvaluatedCount);
    }

    [Fact]
    public void RoundingDecorator_RoundsHalfUp_AndKeepsRawScore()
    {
        var rubric = new List<Criterion> { NewCriterion("C1", 10m, 1m) };
        var calculator = new RoundingGradeDecorator(Service);

        var cases = new (decimal Score, decimal Expected)[] { (4.5m, 5m), (4.4m, 4m), (0.5m, 1m), (10m, 10m), (0m, 0m) };
        foreach (var (score, expected) in cases)
        {
            var evaluations = new List<Evaluation> { Evaluate(rubric, "C1", score) };

            var grade = calculator.Calculate(Ivanov, rubric, evaluations, new WeightedSumStrategy());

            Assert.Equal(expected, grade.FinalScore);
            Assert.Equal(score, grade.RawScore);
        }
    }

    [Fact]
    public void RoundingDecorator_DoesNotRoundTwice()
    {
        // 4.496 нельзя сначала округлить до 4.50, а потом до 5: правильный ответ 4
        var rubric = new List<Criterion> { NewCriterion("C1", 10m, 1m) };
        var evaluations = new List<Evaluation> { Evaluate(rubric, "C1", 4.496m) };
        var calculator = new RoundingGradeDecorator(Service);

        var grade = calculator.Calculate(Ivanov, rubric, evaluations, new WeightedSumStrategy());

        Assert.Equal(4m, grade.FinalScore);
    }

    [Fact]
    public void FinalGrade_CannotLeaveScale()
    {
        Assert.Throws<DomainException>(() => new FinalGrade("S1", 11m, 11m, 1, 1));
        Assert.Throws<DomainException>(() => new FinalGrade("S1", -1m, 0m, 1, 1));
    }
}
