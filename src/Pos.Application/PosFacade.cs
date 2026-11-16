using Pos.Domain;

namespace Pos.Application;

/// <summary>
/// Паттерн Facade (структурный): единая точка входа сценариев для консоли.
/// Консоль не знает про репозитории, фабрику, доменный сервис и экспортёры — она вызывает только методы фасада.
/// </summary>
public class PosFacade
{
    private readonly IStudentRepository _students;
    private readonly ICriterionRepository _criteria;
    private readonly IEvaluationRepository _evaluations;
    private readonly EvaluationFactory _evaluationFactory;
    private readonly IGradeCalculator _gradeCalculator;
    private readonly Dictionary<string, IReportExporter> _exporters;

    public PosFacade(
        IStudentRepository students,
        ICriterionRepository criteria,
        IEvaluationRepository evaluations,
        EvaluationFactory evaluationFactory,
        IGradeCalculator gradeCalculator,
        IEnumerable<IGradeCalculationStrategy> strategies,
        IEnumerable<IReportExporter> exporters)
    {
        _students = students;
        _criteria = criteria;
        _evaluations = evaluations;
        _evaluationFactory = evaluationFactory;
        _gradeCalculator = gradeCalculator;
        _exporters = exporters.ToDictionary(e => e.Format, StringComparer.OrdinalIgnoreCase);

        AvailableStrategies = strategies.ToList();
        if (AvailableStrategies.Count == 0)
            throw new ArgumentException("Не передано ни одного правила расчёта итога.", nameof(strategies));

        CurrentStrategy = AvailableStrategies[0];
    }

    public IReadOnlyList<IGradeCalculationStrategy> AvailableStrategies { get; }

    public IGradeCalculationStrategy CurrentStrategy { get; private set; }

    public IReadOnlyList<string> ExportFormats => _exporters.Keys.ToList();

    // Студенты и рубрикатор

    public Student AddStudent(string id, string fullName)
    {
        var student = new Student(id, fullName);
        if (_students.GetById(student.Id) is not null)
            throw new DomainException($"Студент с ID '{student.Id}' уже существует.");

        _students.Add(student);
        return student;
    }

    public IReadOnlyList<Student> GetAllStudents() => _students.GetAll();

    public Criterion AddCriterion(string id, string title, decimal maxScore, decimal weight)
    {
        var criterion = new Criterion(id, title, new Score(maxScore), new Weight(weight));
        if (_criteria.GetById(criterion.Id) is not null)
            throw new DomainException($"Критерий с ID '{criterion.Id}' уже есть в рубрикаторе.");

        _criteria.Add(criterion);
        return criterion;
    }

    public IReadOnlyList<Criterion> GetRubric() => _criteria.GetAll();

    // Оценки: выставить, исправить, отменить

    public Evaluation EvaluateStudent(
        string evalId, string studentId, string criterionId, decimal score, string comment = "")
    {
        var student = _students.GetById(studentId)
            ?? throw new DomainException($"Студент '{studentId}' не найден.");

        var evaluation = _evaluationFactory.Create(
            evalId,
            student,
            _criteria.GetAll(),
            criterionId,
            new Score(score),
            _evaluations.GetByStudentId(student.Id),
            comment);

        if (_evaluations.GetById(evaluation.Id) is not null)
            throw new DomainException($"Оценка с ID '{evaluation.Id}' уже существует.");

        _evaluations.Add(evaluation);
        return evaluation;
    }

    public void CorrectEvaluation(string evalId, decimal newScore, string comment = "")
    {
        var evaluation = _evaluations.GetById(evalId)
            ?? throw new DomainException($"Оценка '{evalId}' не найдена.");
        var criterion = _criteria.GetById(evaluation.CriterionId)
            ?? throw new DomainException($"Критерий '{evaluation.CriterionId}' не найден.");

        evaluation.Correct(new Score(newScore), criterion, comment);
        _evaluations.Update(evaluation);
    }

    public void CancelEvaluation(string evalId)
    {
        if (!_evaluations.Remove(evalId))
            throw new DomainException($"Оценка '{evalId}' не найдена.");
    }

    public IReadOnlyList<Evaluation> GetAllEvaluations() => _evaluations.GetAll();

    // Итог и экспорт

    public void SelectStrategy(IGradeCalculationStrategy strategy)
    {
        if (!AvailableStrategies.Contains(strategy))
            throw new ArgumentException("Такого правила расчёта нет в списке доступных.", nameof(strategy));

        CurrentStrategy = strategy;
    }

    public FinalGrade CalculateStudentGrade(string studentId)
    {
        var student = _students.GetById(studentId)
            ?? throw new DomainException($"Студент '{studentId}' не найден.");

        return Calculate(student, _criteria.GetAll());
    }

    public IReadOnlyList<StudentReportRow> GetSummaryTable()
    {
        var rubric = _criteria.GetAll();
        var rows = new List<StudentReportRow>();

        foreach (var student in _students.GetAll())
        {
            var grade = Calculate(student, rubric);
            rows.Add(new StudentReportRow(
                student.Id,
                student.FullName,
                Math.Round(grade.RawScore, 2, MidpointRounding.AwayFromZero),
                grade.FinalScore,
                grade.EvaluatedCount,
                grade.TotalCriteriaCount));
        }

        return rows;
    }

    public string ExportReport(string format, string? filePath = null)
    {
        if (!_exporters.TryGetValue(format.Trim(), out var exporter))
            throw new DomainException($"Формат '{format}' не поддерживается. Доступно: {string.Join(", ", ExportFormats)}.");

        return exporter.Export(GetSummaryTable(), filePath);
    }

    private FinalGrade Calculate(Student student, IReadOnlyList<Criterion> rubric) =>
        _gradeCalculator.Calculate(student, rubric, _evaluations.GetByStudentId(student.Id), CurrentStrategy);
}
