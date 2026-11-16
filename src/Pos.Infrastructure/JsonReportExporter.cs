using System.Text.Encodings.Web;
using System.Text.Json;
using Pos.Application;

namespace Pos.Infrastructure;

/// <summary>
/// Выгрузка ведомости в JSON.
/// </summary>
public class JsonReportExporter : ReportExporterBase
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // кириллица без \uXXXX
    };

    public override string Format => "JSON";

    protected override string BuildContent(IReadOnlyList<StudentReportRow> rows) =>
        JsonSerializer.Serialize(rows, Options);
}
