using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

/// <summary>
/// Выгрузка ведомости в CSV (разделитель «;», как ждёт русская локаль Excel).
/// </summary>
public class CsvReportExporter : ReportExporterBase
{
    public override string Format => "CSV";

    // UTF-8 с BOM, чтобы Excel правильно открывал кириллицу
    protected override Encoding FileEncoding => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    protected override string BuildContent(IReadOnlyList<StudentReportRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("StudentId;FullName;RawScore;FinalScore;EvaluatedCriteria");

        foreach (var row in rows)
        {
            var raw = row.RawScore.ToString("0.00", CultureInfo.InvariantCulture);
            var final = row.FinalScore.ToString("0", CultureInfo.InvariantCulture);
            sb.AppendLine(
                $"{Escape(row.StudentId)};{Escape(row.FullName)};{raw};{final};" +
                $"{row.EvaluatedCount}/{row.TotalCriteriaCount}");
        }

        return sb.ToString().TrimEnd();
    }

    // Если в значении есть «;», кавычки или перенос строки, его нужно взять в кавычки
    private static string Escape(string value)
    {
        bool needsQuotes = value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        return needsQuotes ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }
}
