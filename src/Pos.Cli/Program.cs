using System.Globalization;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Pos.Cli;

public static class Program
{
    public static void Main(string[] args)
    {
        ConfigureConsole();

        using var provider = CompositionRoot.BuildServiceProvider();

        if (args.Any(a => a == "--demo"))
            RunDemo(provider.GetRequiredService<DemoScenario>());
        else
            provider.GetRequiredService<ConsoleApp>().Run();
    }

    private static void ConfigureConsole()
    {
        // Числа везде с точкой (как в подсказках ввода, CSV и JSON), независимо от региональных настроек ОС.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            // Кириллица и в выводе, и во вводе (в том числе при перенаправленном вводе).
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch (IOException)
        {
            // Консоли нет (например, запуск без терминала) — остаётся кодировка по умолчанию.
        }
    }

    private static void RunDemo(DemoScenario demo)
    {
        try
        {
            demo.Run();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"[Ошибка файла] {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
