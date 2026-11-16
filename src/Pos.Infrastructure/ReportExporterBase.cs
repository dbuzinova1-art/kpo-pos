using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

/// <summary>
/// Паттерн Template Method (поведенческий): общий каркас выгрузки
/// (проверка -> формирование текста -> запись в файл), а формат текста задаёт наследник.
/// Благодаря этому CSV и JSON не дублируют работу с файлом.
/// </summary>
public abstract class ReportExporterBase : IReportExporter
{
    public abstract string Format { get; }

    /// <summary>
    /// Кодировка файла. Необязательный шаг шаблона: по умолчанию UTF-8 без BOM
    /// (так требуется, например, для JSON), наследник может переопределить.
    /// </summary>
    protected virtual Encoding FileEncoding => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public string Export(IReadOnlyList<StudentReportRow> rows, string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(rows);

        string content = BuildContent(rows);

        if (!string.IsNullOrWhiteSpace(filePath))
            File.WriteAllText(filePath, content, FileEncoding);

        return content;
    }

    /// <summary>
    /// Изменяемый шаг шаблона: как записать строки ведомости в текст.
    /// </summary>
    protected abstract string BuildContent(IReadOnlyList<StudentReportRow> rows);
}
