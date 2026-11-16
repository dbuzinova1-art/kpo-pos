using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.Cli;

/// <summary>
/// Composition root: единственное место, где собирается граф зависимостей.
/// Только здесь знают, какие адаптеры подставлены вместо портов.
/// </summary>
public static class CompositionRoot
{
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Порты домена -> адаптеры хранения
        services.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
        services.AddSingleton<ICriterionRepository, InMemoryCriterionRepository>();
        services.AddSingleton<IEvaluationRepository, InMemoryEvaluationRepository>();

        // Домен: фабрика, правила расчёта (Strategy), доменный сервис в обёртке Decorator
        services.AddSingleton<EvaluationFactory>();
        services.AddSingleton<IGradeCalculationStrategy, WeightedSumStrategy>();
        services.AddSingleton<IGradeCalculationStrategy, SimpleAverageStrategy>();
        services.AddSingleton<GradeCalculationService>();
        services.AddSingleton<IGradeCalculator>(sp =>
            new RoundingGradeDecorator(sp.GetRequiredService<GradeCalculationService>()));

        // Application: выходной порт экспорта и фасад сценариев
        services.AddSingleton<IReportExporter, CsvReportExporter>();
        services.AddSingleton<IReportExporter, JsonReportExporter>();
        services.AddSingleton<PosFacade>();

        // Консольный интерфейс
        services.AddSingleton<DemoScenario>();
        services.AddSingleton<ConsoleApp>();

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }
}
