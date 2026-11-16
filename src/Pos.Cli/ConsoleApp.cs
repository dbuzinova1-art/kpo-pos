using System.Globalization;
using Pos.Application;
using Pos.Domain;

namespace Pos.Cli;

/// <summary>
/// Консольное меню. Работает только с фасадом приложения.
/// </summary>
public class ConsoleApp
{
    private readonly PosFacade _facade;
    private readonly DemoScenario _demo;

    public ConsoleApp(PosFacade facade, DemoScenario demo)
    {
        _facade = facade;
        _demo = demo;
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();

            var choice = Console.ReadLine()?.Trim();
            if (choice is null or "0")
                break;

            try
            {
                Dispatch(choice);
            }
            catch (DomainException ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"[Ошибка ввода] {ex.Message}");
            }
            catch (OverflowException)
            {
                Console.WriteLine("[Ошибка ввода] Слишком большое число.");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                           or ArgumentException or NotSupportedException)
            {
                Console.WriteLine($"[Ошибка файла] {ex.Message}");
            }
        }

        Console.WriteLine("Работа завершена.");
    }

    private void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== ПОС: Преподавательская Оценка Студентов ===");
        Console.WriteLine($"Правило итога: {_facade.CurrentStrategy.Title}; итог округляется до целого");
        Console.WriteLine("1. Студенты (добавить / список)");
        Console.WriteLine("2. Критерии (добавить / список)");
        Console.WriteLine("3. Оценки (выставить / исправить / отменить / список)");
        Console.WriteLine("4. Итоговая ведомость");
        Console.WriteLine("5. Сменить правило расчёта итога");
        Console.WriteLine("6. Экспорт итога (CSV / JSON)");
        Console.WriteLine("7. Демонстрационный сценарий");
        Console.WriteLine("0. Выход");
        Console.Write("Выбор: ");
    }

    private void Dispatch(string choice)
    {
        switch (choice)
        {
            case "1": HandleStudents(); break;
            case "2": HandleCriteria(); break;
            case "3": HandleEvaluations(); break;
            case "4": ConsoleView.PrintSummary(_facade.GetSummaryTable()); break;
            case "5": HandleStrategy(); break;
            case "6": HandleExport(); break;
            case "7": RunDemo(); break;
            default: PrintUnknownChoice(); break;
        }
    }

    private static void PrintUnknownChoice() =>
        Console.WriteLine("Неизвестный пункт. Введите номер из предложенных.");

    private void HandleStudents()
    {
        switch (Ask("1 - добавить, 2 - список: "))
        {
            case "1":
            {
                var student = _facade.AddStudent(Ask("ID студента: "), Ask("ФИО: "));
                Console.WriteLine($"Студент добавлен: [{student.Id}] {student.FullName}.");
                break;
            }
            case "2":
                ConsoleView.PrintStudents(_facade.GetAllStudents());
                break;
            default:
                PrintUnknownChoice();
                break;
        }
    }

    private void HandleCriteria()
    {
        switch (Ask("1 - добавить, 2 - список: "))
        {
            case "1":
            {
                var criterion = _facade.AddCriterion(
                    Ask("ID критерия: "),
                    Ask("Название: "),
                    AskDecimal("Максимальный балл: "),
                    AskDecimal("Вес (например, 0.3): "));
                Console.WriteLine($"Критерий добавлен: [{criterion.Id}] {criterion.Title}.");
                break;
            }
            case "2":
                ConsoleView.PrintRubric(_facade.GetRubric());
                break;
            default:
                PrintUnknownChoice();
                break;
        }
    }

    private void HandleEvaluations()
    {
        switch (Ask("1 - выставить, 2 - исправить, 3 - отменить, 4 - список: "))
        {
            case "1":
            {
                // Показываем, какие ID можно вводить
                Console.WriteLine("Студенты:");
                ConsoleView.PrintStudents(_facade.GetAllStudents());
                Console.WriteLine("Критерии:");
                ConsoleView.PrintRubric(_facade.GetRubric());

                var evaluation = _facade.EvaluateStudent(
                    Ask("ID оценки (придумайте, например EV-1): "),
                    Ask("ID студента: "),
                    Ask("ID критерия: "),
                    AskDecimal("Балл: "),
                    Ask("Комментарий (можно пусто): "));
                Console.WriteLine(
                    $"Оценка выставлена: [{evaluation.Id}] студент {evaluation.StudentId}, " +
                    $"критерий {evaluation.CriterionId}, балл {evaluation.Score}.");
                break;
            }
            case "2":
                ConsoleView.PrintEvaluations(_facade.GetAllEvaluations());
                _facade.CorrectEvaluation(
                    Ask("ID оценки: "),
                    AskDecimal("Новый балл: "),
                    Ask("Комментарий (можно пусто): "));
                Console.WriteLine("Оценка исправлена.");
                break;
            case "3":
                ConsoleView.PrintEvaluations(_facade.GetAllEvaluations());
                _facade.CancelEvaluation(Ask("ID оценки: "));
                Console.WriteLine("Оценка отменена.");
                break;
            case "4":
                ConsoleView.PrintEvaluations(_facade.GetAllEvaluations());
                break;
            default:
                PrintUnknownChoice();
                break;
        }
    }

    private void HandleStrategy()
    {
        var strategies = _facade.AvailableStrategies;
        for (var i = 0; i < strategies.Count; i++)
            Console.WriteLine($"  {i + 1}. {strategies[i].Title}");

        if (!int.TryParse(Ask("Номер правила: "), out var number) || number < 1 || number > strategies.Count)
        {
            Console.WriteLine("Такого правила нет.");
            return;
        }

        _facade.SelectStrategy(strategies[number - 1]);
        Console.WriteLine($"Выбрано правило: {_facade.CurrentStrategy.Title}");
    }

    private void HandleExport()
    {
        var format = Ask($"Формат ({string.Join(" / ", _facade.ExportFormats)}): ");
        var path = Ask("Файл для сохранения (Enter - только показать на экране): ");

        Console.WriteLine(_facade.ExportReport(format, path));

        if (path.Length > 0)
            Console.WriteLine($"Сохранено: {Path.GetFullPath(path)}");
    }

    private void RunDemo()
    {
        // Демо заводит свои данные, поэтому запускаем его только на пустом состоянии
        if (_facade.GetAllStudents().Count > 0 || _facade.GetRubric().Count > 0)
        {
            Console.WriteLine("Демо рассчитано на пустые данные. Запустите программу заново с ключом --demo.");
            return;
        }

        _demo.Run();
    }

    private static string Ask(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    private static decimal AskDecimal(string prompt)
    {
        var text = Ask(prompt).Replace(',', '.');
        if (!decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException("Нужно ввести число, например 7.5.");
        }

        return value;
    }
}
