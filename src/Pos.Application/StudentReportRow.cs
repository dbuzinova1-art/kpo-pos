namespace Pos.Application;

/// <summary>
/// Строка итоговой ведомости (DTO для вывода и экспорта).
/// </summary>
public record StudentReportRow(
    string StudentId,
    string FullName,
    decimal RawScore,
    decimal FinalScore,
    int EvaluatedCount,
    int TotalCriteriaCount);
