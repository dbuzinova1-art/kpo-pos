using Pos.Application;
using Pos.Domain;

namespace Pos.Cli;

/// <summary>
/// Сквозной сценарий из ТЗ: студенты и критерии -> оценки -> итог -> экспорт.
/// </summary>
public class DemoScenario
{
    private readonly PosFacade _facade;

    public DemoScenario(PosFacade facade)
    {
        _facade = facade;
    }

    public void Run(string exportDirectory = ".")
    {
        Console.WriteLine();
        Console.WriteLine("=== Демо: студенты -> критерии -> оценки -> итог -> экспорт ===");

        Console.WriteLine();
        Console.WriteLine("1. Заводим студентов:");
        _facade.AddStudent("ST-1", "Иванов Алексей");
        _facade.AddStudent("ST-2", "Смирнова Дарья");
        ConsoleView.PrintStudents(_facade.GetAllStudents());

        // Рубрикатор по формуле: 0.2 * коллоквиум + 0.8 * (0.2*ДЗ1 + 0.2*ДЗ2 + 0.3*ДЗ3 + 0.3*ДЗ4),
        // поэтому веса ДЗ уже умножены на 0.8 и в сумме все веса дают 1.
        Console.WriteLine();
        Console.WriteLine("2. Заводим критерии (рубрикатор):");
        _facade.AddCriterion("COLLOQ", "Коллоквиум", maxScore: 10m, weight: 0.20m);
        _facade.AddCriterion("HW-1", "ДЗ-1", maxScore: 10m, weight: 0.16m);
        _facade.AddCriterion("HW-2", "ДЗ-2", maxScore: 10m, weight: 0.16m);
        _facade.AddCriterion("HW-3", "ДЗ-3", maxScore: 10m, weight: 0.24m);
        _facade.AddCriterion("HW-4", "ДЗ-4", maxScore: 10m, weight: 0.24m);
        ConsoleView.PrintRubric(_facade.GetRubric());

        Console.WriteLine();
        Console.WriteLine("3. Выставляем оценки (у Смирновой пока только два критерия):");
        _facade.EvaluateStudent("EV-1", "ST-1", "COLLOQ", 8m, "Устный ответ");
        _facade.EvaluateStudent("EV-2", "ST-1", "HW-1", 9m);
        _facade.EvaluateStudent("EV-3", "ST-1", "HW-2", 10m);
        _facade.EvaluateStudent("EV-4", "ST-1", "HW-3", 8m);
        _facade.EvaluateStudent("EV-5", "ST-1", "HW-4", 9m);
        _facade.EvaluateStudent("EV-6", "ST-2", "HW-1", 10m);
        _facade.EvaluateStudent("EV-7", "ST-2", "HW-2", 10m);
        ConsoleView.PrintEvaluations(_facade.GetAllEvaluations());

        Console.WriteLine();
        Console.WriteLine("4. Пробуем нарушить правила домена:");
        TryViolate("12 баллов из 10", () => _facade.EvaluateStudent("EV-X1", "ST-2", "HW-3", 12m));
        TryViolate("отрицательный балл", () => _facade.EvaluateStudent("EV-X2", "ST-2", "HW-3", -1m));
        TryViolate("критерия нет в рубрикаторе", () => _facade.EvaluateStudent("EV-X3", "ST-2", "UNKNOWN", 5m));
        TryViolate("вторая оценка по тому же критерию", () => _facade.EvaluateStudent("EV-X4", "ST-1", "HW-1", 7m));

        Console.WriteLine();
        Console.WriteLine("5. Исправляем EV-1 после апелляции (с 8 на 9) и отменяем ошибочную оценку:");
        _facade.CorrectEvaluation("EV-1", 9m, "После апелляции");
        _facade.EvaluateStudent("EV-8", "ST-2", "COLLOQ", 3m, "Выставлена по ошибке");
        _facade.CancelEvaluation("EV-8");
        ConsoleView.PrintEvaluations(_facade.GetAllEvaluations());

        Console.WriteLine();
        Console.WriteLine($"6. Итоговая ведомость. Правило: {_facade.CurrentStrategy.Title}, округление до целого:");
        ConsoleView.PrintSummary(_facade.GetSummaryTable());

        Console.WriteLine();
        Console.WriteLine("7. Экспорт итога:");
        var csvPath = Path.Combine(exportDirectory, "pos_report.csv");
        var jsonPath = Path.Combine(exportDirectory, "pos_report.json");
        Console.WriteLine("--- CSV ---");
        Console.WriteLine(_facade.ExportReport("CSV", csvPath));
        Console.WriteLine("--- JSON ---");
        Console.WriteLine(_facade.ExportReport("JSON", jsonPath));
        Console.WriteLine($"Файлы сохранены: {Path.GetFullPath(csvPath)} и {Path.GetFullPath(jsonPath)}");
    }

    private static void TryViolate(string what, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"  !!! {what}: правило не сработало");
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"  {what} -> отказ: {ex.Message}");
        }
    }
}
