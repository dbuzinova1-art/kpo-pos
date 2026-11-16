using Pos.Application;
using Pos.Domain;

namespace Pos.Cli;

/// <summary>
/// Форматированный вывод списков и ведомости в консоль.
/// </summary>
public static class ConsoleView
{
    public static void PrintStudents(IReadOnlyList<Student> students)
    {
        if (students.Count == 0)
        {
            Console.WriteLine("  (студентов пока нет)");
            return;
        }

        foreach (var s in students)
            Console.WriteLine($"  [{s.Id}] {s.FullName}");
    }

    public static void PrintRubric(IReadOnlyList<Criterion> rubric)
    {
        if (rubric.Count == 0)
        {
            Console.WriteLine("  (рубрикатор пока пуст)");
            return;
        }

        foreach (var c in rubric)
            Console.WriteLine($"  [{c.Id}] {c.Title} (макс. балл: {c.MaxScore}, вес: {c.Weight})");

        var totalWeight = rubric.Sum(c => c.Weight.Value);
        Console.WriteLine($"  Сумма весов: {totalWeight:0.##} (итог считается пропорционально весам)");
    }

    public static void PrintEvaluations(IReadOnlyList<Evaluation> evaluations)
    {
        if (evaluations.Count == 0)
        {
            Console.WriteLine("  (оценок пока нет)");
            return;
        }

        foreach (var e in evaluations)
        {
            var comment = e.Comment.Length > 0 ? $" — {e.Comment}" : string.Empty;
            Console.WriteLine(
                $"  [{e.Id}] студент {e.StudentId}, критерий {e.CriterionId}: {e.Score} " +
                $"({e.Date.ToLocalTime():dd.MM.yyyy HH:mm}){comment}");
        }
    }

    public static void PrintSummary(IReadOnlyList<StudentReportRow> rows)
    {
        if (rows.Count == 0)
        {
            Console.WriteLine("  (студентов пока нет, ведомость пуста)");
            return;
        }

        Console.WriteLine($"{"ID",-8} | {"Студент",-20} | {"Сырой балл",-10} | {"Итог",-5} | Оценено критериев");
        Console.WriteLine(new string('-', 72));

        foreach (var r in rows)
        {
            Console.WriteLine(
                $"{r.StudentId,-8} | {r.FullName,-20} | {r.RawScore,-10:0.00} | {r.FinalScore,-5:0} | " +
                $"{r.EvaluatedCount} из {r.TotalCriteriaCount}");
        }
    }
}
