namespace Pos.Application;

/// <summary>
/// Выходной порт: выгрузка итоговой ведомости. Форматы (CSV, JSON) реализует Infrastructure.
/// </summary>
public interface IReportExporter
{
    string Format { get; }

    /// <summary>
    /// Формирует ведомость, при заданном filePath сохраняет её в файл. Возвращает текст ведомости.
    /// </summary>
    string Export(IReadOnlyList<StudentReportRow> rows, string? filePath = null);
}
