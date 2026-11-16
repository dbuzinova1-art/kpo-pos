using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Cli;
using Pos.Domain;
using Xunit;

namespace Pos.Tests;

/// <summary>
/// Сценарии через фасад и DI-контейнер: так же, как работает консольное приложение.
/// </summary>
public class ScenarioTests
{
    private static PosFacade CreateFacade() =>
        CompositionRoot.BuildServiceProvider().GetRequiredService<PosFacade>();

    [Fact]
    public void Container_BuildsWholeGraph_AndWrapsCalculatorWithDecorator()
    {
        using var provider = CompositionRoot.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<ConsoleApp>());
        Assert.IsType<RoundingGradeDecorator>(provider.GetRequiredService<IGradeCalculator>());
        Assert.Equal(2, provider.GetServices<IGradeCalculationStrategy>().Count());
        Assert.Equal(2, provider.GetServices<IReportExporter>().Count());
    }

    [Fact]
    public void Facade_RejectsViolationsOfDomainRules()
    {
        var facade = CreateFacade();
        facade.AddStudent("S1", "Иванов");
        facade.AddCriterion("C1", "ДЗ-1", maxScore: 10m, weight: 0.5m);

        Assert.Throws<DomainException>(() => facade.EvaluateStudent("E1", "S1", "C1", 12m));
        Assert.Throws<DomainException>(() => facade.EvaluateStudent("E1", "S1", "C1", -1m));
        Assert.Throws<DomainException>(() => facade.EvaluateStudent("E1", "S1", "NO-SUCH", 5m));
        Assert.Throws<DomainException>(() => facade.EvaluateStudent("E1", "NO-SUCH", "C1", 5m));
        Assert.Empty(facade.GetAllEvaluations());
    }

    [Fact]
    public void Facade_RejectsDuplicates()
    {
        var facade = CreateFacade();
        facade.AddStudent(" S1 ", "Иванов");
        facade.AddCriterion("C1", "ДЗ-1", 10m, 0.5m);
        facade.AddCriterion("C2", "ДЗ-2", 10m, 0.5m);
        facade.EvaluateStudent("E1", "S1", "C1", 5m);

        Assert.Throws<DomainException>(() => facade.AddStudent("s1", "Другой"));
        Assert.Throws<DomainException>(() => facade.AddCriterion("c1", "Ещё раз", 10m, 0.5m));
        Assert.Throws<DomainException>(() => facade.EvaluateStudent("E1", "S1", "C2", 5m)); // ID оценки занят
        Assert.Single(facade.GetAllStudents());
    }

    [Fact]
    public void Evaluation_CanBeCorrectedAndCancelled_AndGradeFollows()
    {
        var facade = CreateFacade();
        facade.AddStudent("S1", "Иванов");
        facade.AddCriterion("C1", "ДЗ-1", 10m, 1m);

        facade.EvaluateStudent("E1", "S1", "C1", 6m);
        Assert.Equal(6m, facade.CalculateStudentGrade("S1").RawScore);

        facade.CorrectEvaluation("E1", 10m, "Исправлено");
        Assert.Equal(10m, facade.CalculateStudentGrade("S1").RawScore);
        Assert.Throws<DomainException>(() => facade.CorrectEvaluation("E1", 11m));

        facade.CancelEvaluation("E1");
        Assert.Equal(0m, facade.CalculateStudentGrade("S1").RawScore);
        Assert.Throws<DomainException>(() => facade.CancelEvaluation("E1"));
    }

    [Fact]
    public void Facade_CalculatesRoundedFinalGrade()
    {
        var facade = CreateFacade();
        facade.AddStudent("S1", "Иванов");
        facade.AddCriterion("C1", "ДЗ-1", 10m, 0.3m);
        facade.AddCriterion("C2", "ДЗ-2", 10m, 0.4m);
        facade.AddCriterion("C3", "Семинары", 10m, 0.3m);
        facade.EvaluateStudent("E1", "S1", "C1", 8.5m);
        facade.EvaluateStudent("E2", "S1", "C2", 9m);
        facade.EvaluateStudent("E3", "S1", "C3", 9m);

        var grade = facade.CalculateStudentGrade("S1");

        Assert.Equal(8.85m, grade.RawScore);
        Assert.Equal(9m, grade.FinalScore);
    }

    [Fact]
    public void Facade_CanSwitchStrategy()
    {
        var facade = CreateFacade();
        facade.AddStudent("S1", "Иванов");
        facade.AddCriterion("C1", "ДЗ-1", 10m, 0.9m);
        facade.AddCriterion("C2", "ДЗ-2", 10m, 0.1m);
        facade.EvaluateStudent("E1", "S1", "C1", 10m);
        facade.EvaluateStudent("E2", "S1", "C2", 0m);

        Assert.Equal(9m, facade.CalculateStudentGrade("S1").FinalScore);

        facade.SelectStrategy(facade.AvailableStrategies[1]);

        Assert.Equal(5m, facade.CalculateStudentGrade("S1").FinalScore);
    }

    [Fact]
    public void Export_ProducesCsvAndJson()
    {
        var facade = CreateFacade();
        facade.AddStudent("S1", "Иванов; Иван");
        facade.AddCriterion("C1", "ДЗ-1", 10m, 1m);
        facade.EvaluateStudent("E1", "S1", "C1", 8.5m); // 8.5 -> округляется до 9

        var csv = facade.ExportReport("CSV");
        var json = facade.ExportReport("json");

        Assert.Contains("S1;\"Иванов; Иван\";8.50;9;1/1", csv);

        using var document = JsonDocument.Parse(json);
        var row = document.RootElement[0];
        Assert.Equal("S1", row.GetProperty("StudentId").GetString());
        Assert.Equal(8.5m, row.GetProperty("RawScore").GetDecimal());
        Assert.Equal(9m, row.GetProperty("FinalScore").GetDecimal());

        Assert.Throws<DomainException>(() => facade.ExportReport("XML"));
    }

    [Fact]
    public void Export_WritesFile()
    {
        var facade = CreateFacade();
        facade.AddStudent("S1", "Иванов");
        var directory = Directory.CreateTempSubdirectory();

        try
        {
            var path = Path.Combine(directory.FullName, "report.csv");
            facade.ExportReport("CSV", path);

            Assert.Contains("S1;Иванов", File.ReadAllText(path));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Export_CsvFileHasBom_JsonFileHasNot()
    {
        var facade = CreateFacade();
        facade.AddStudent("S1", "Иванов");
        var directory = Directory.CreateTempSubdirectory();

        try
        {
            var csvPath = Path.Combine(directory.FullName, "report.csv");
            var jsonPath = Path.Combine(directory.FullName, "report.json");
            facade.ExportReport("CSV", csvPath);
            facade.ExportReport("JSON", jsonPath);

            // CSV с BOM открывается в Excel с кириллицей, а JSON с BOM строгие парсеры не принимают
            Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, File.ReadAllBytes(csvPath).Take(3).ToArray());
            Assert.Equal((byte)'[', File.ReadAllBytes(jsonPath)[0]);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void ConsoleApp_SurvivesBadInput_AndStopsAtEndOfInput()
    {
        using var provider = CompositionRoot.BuildServiceProvider();
        var app = provider.GetRequiredService<ConsoleApp>();
        var input = string.Join(
            Environment.NewLine,
            "1", "1", "ST-1", "Иванов Алексей",         // добавить студента
            "1", "9",                                    // неизвестный подпункт
            "2", "1", "C1", "ДЗ-1", "abc",               // максимум не число
            "2", "1", "C1", "ДЗ-1", "10", "0,5",         // критерий (запятая как разделитель)
            "3", "1", "E1", "ST-1", "C1", "12", "",      // 12 баллов из 10
            "42");                                       // неизвестный пункт меню, дальше конец ввода

        var output = RunConsole(app, input);

        Assert.Contains("Студент добавлен: [ST-1] Иванов Алексей.", output);
        Assert.Contains("Неизвестный пункт", output);
        Assert.Contains("[Ошибка ввода] Нужно ввести число", output);
        Assert.Contains("Критерий добавлен: [C1] ДЗ-1.", output);
        Assert.Contains("[Ошибка] Нельзя поставить 12 из 10", output);
        Assert.Contains("Работа завершена.", output);
    }

    private static string RunConsole(ConsoleApp app, string input)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var output = new StringWriter();

        try
        {
            Console.SetIn(new StringReader(input));
            Console.SetOut(output);
            app.Run();
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }

        return output.ToString();
    }

    [Fact]
    public void Demo_RunsWithoutErrors()
    {
        using var provider = CompositionRoot.BuildServiceProvider();
        var demo = provider.GetRequiredService<DemoScenario>();
        var directory = Directory.CreateTempSubdirectory();
        var originalOut = Console.Out;

        try
        {
            Console.SetOut(new StringWriter());
            demo.Run(directory.FullName);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        try
        {
            var facade = provider.GetRequiredService<PosFacade>();
            var rows = facade.GetSummaryTable();

            Assert.Equal(2, rows.Count);
            Assert.Equal(9m, rows[0].FinalScore); // Иванов: 8.92 -> 9
            Assert.Equal(3m, rows[1].FinalScore); // Смирнова: 3.20 -> 3
            Assert.True(File.Exists(Path.Combine(directory.FullName, "pos_report.csv")));
            Assert.True(File.Exists(Path.Combine(directory.FullName, "pos_report.json")));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
