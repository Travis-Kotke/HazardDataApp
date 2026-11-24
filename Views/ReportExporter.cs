using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using HazardDataApp.BusinessLogic.Models;

namespace HazardDataApp.UI.Views;

internal static class ReportExporter
{
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private static readonly double[] ColumnWidths = [26, 60, 16, 12, 30, 16, 20, 12];

    private static readonly string[] Headers =
    [
        "Incident title",
        "Description",
        "Severity",
        "Public",
        "Location",
        "Date reported",
        "Resolution status",
        "Flagged"
    ];

    public static async Task<string> ExportCsvAsync(IReadOnlyList<Report> reports)
    {
        var filePath = CreateFilePath("csv");
        var content = string.Join(
            "\r\n",
            GetRows(reports).Select(row => string.Join(",", row.Select(EscapeCsvField))));
        await File.WriteAllTextAsync(filePath, content + "\r\n", new UTF8Encoding(true));
        return filePath;
    }

    public static Task<string> ExportExcelAsync(IReadOnlyList<Report> reports)
    {
        var filePath = CreateFilePath("xlsx");
        using (var archive = ZipFile.Open(filePath, ZipArchiveMode.Create))
        {
            WriteEntry(archive, "[Content_Types].xml",
                """<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>""");
            WriteEntry(archive, "_rels/.rels",
                """<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");
            WriteEntry(archive, "xl/workbook.xml",
                $"""<workbook xmlns="{SpreadsheetNamespace}" xmlns:r="{RelationshipsNamespace}"><sheets><sheet name="Hazard Reports" sheetId="1" r:id="rId1"/></sheets></workbook>""");
            WriteEntry(archive, "xl/_rels/workbook.xml.rels",
                """<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>""");
            WriteEntry(archive, "xl/worksheets/sheet1.xml", CreateWorksheet(reports));
            WriteEntry(archive, "xl/styles.xml", CreateStyles());
        }

        return Task.FromResult(filePath);
    }

    private static string CreateFilePath(string extension) =>
        Path.Combine(FileSystem.CacheDirectory, $"hazard-reports-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.{extension}");

    private static IEnumerable<string[]> GetRows(IReadOnlyList<Report> reports)
    {
        yield return Headers;
        foreach (var report in reports)
        {
            yield return
            [
                TextOrEmpty(report.Title),
                TextOrEmpty(report.Description),
                TextOrEmpty(report.Severity),
                report.Published ? "Yes" : "No",
                TextOrEmpty(report.Location),
                report.DateReported.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                report.IsResolved ? "Resolved" : "Unresolved",
                report.IsFlagged ? "Yes" : "No"
            ];
        }
    }

    private static string EscapeCsvField(string value)
    {
        var firstNonWhitespace = value.FirstOrDefault(character => !char.IsWhiteSpace(character));
        if (firstNonWhitespace is '=' or '+' or '-' or '@')
            value = "'" + value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string TextOrEmpty(string? value) => value ?? string.Empty;

    private static string CreateWorksheet(IReadOnlyList<Report> reports)
    {
        var builder = new StringBuilder();
        using var writer = XmlWriter.Create(builder, new XmlWriterSettings { OmitXmlDeclaration = true });
        writer.WriteStartElement("worksheet", SpreadsheetNamespace);
        writer.WriteStartElement("sheetViews", SpreadsheetNamespace);
        writer.WriteStartElement("sheetView", SpreadsheetNamespace);
        writer.WriteAttributeString("workbookViewId", "0");
        writer.WriteStartElement("pane", SpreadsheetNamespace);
        writer.WriteAttributeString("ySplit", "1");
        writer.WriteAttributeString("topLeftCell", "A2");
        writer.WriteAttributeString("activePane", "bottomLeft");
        writer.WriteAttributeString("state", "frozen");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteStartElement("cols", SpreadsheetNamespace);
        for (var index = 0; index < ColumnWidths.Length; index++)
        {
            writer.WriteStartElement("col", SpreadsheetNamespace);
            writer.WriteAttributeString("min", (index + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("max", (index + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("width", ColumnWidths[index].ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("customWidth", "1");
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteStartElement("sheetData", SpreadsheetNamespace);

        var rowNumber = 0;
        foreach (var values in GetRows(reports))
        {
            rowNumber++;
            writer.WriteStartElement("row", SpreadsheetNamespace);
            writer.WriteAttributeString("r", rowNumber.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("s", rowNumber == 1 ? "1" : "2");
            if (rowNumber == 1)
            {
                writer.WriteAttributeString("ht", "30");
                writer.WriteAttributeString("customHeight", "1");
            }
            for (var columnIndex = 0; columnIndex < values.Length; columnIndex++)
            {
                writer.WriteStartElement("c", SpreadsheetNamespace);
                writer.WriteAttributeString(
                    "r",
                    $"{GetColumnName(columnIndex + 1)}{rowNumber.ToString(CultureInfo.InvariantCulture)}");
                writer.WriteAttributeString("t", "inlineStr");
                writer.WriteAttributeString("s", rowNumber == 1 ? "1" : "2");
                writer.WriteStartElement("is", SpreadsheetNamespace);
                writer.WriteStartElement("t", SpreadsheetNamespace);
                writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
                writer.WriteString(values[columnIndex]);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteStartElement("autoFilter", SpreadsheetNamespace);
        writer.WriteAttributeString("ref", $"A1:{GetColumnName(Headers.Length)}{rowNumber}");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.Flush();
        return builder.ToString();
    }

    private static string CreateStyles() =>
        $"""<styleSheet xmlns="{SpreadsheetNamespace}"><fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><color rgb="FFFFFFFF"/><sz val="11"/><name val="Calibri"/></font></fonts><fills count="3"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FF512BD4"/><bgColor indexed="64"/></patternFill></fill></fills><borders count="2"><border><left/><right/><top/><bottom/><diagonal/></border><border><left style="thin"><color rgb="FFDADAE2"/></left><right style="thin"><color rgb="FFDADAE2"/></right><top style="thin"><color rgb="FFDADAE2"/></top><bottom style="thin"><color rgb="FFDADAE2"/></bottom><diagonal/></border></borders><cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="3"><xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf><xf numFmtId="0" fontId="1" fillId="2" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment vertical="center" wrapText="1"/></xf><xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf></cellXfs><cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles></styleSheet>""";

    private static string GetColumnName(int columnNumber)
    {
        var name = new StringBuilder();
        while (columnNumber > 0)
        {
            columnNumber--;
            name.Insert(0, (char)('A' + columnNumber % 26));
            columnNumber /= 26;
        }
        return name.ToString();
    }

    private static void WriteEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }
}
