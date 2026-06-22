using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LibroFiscal.Desktop.Services;

/// <summary>
/// ponytail: Generador de CSV nativo ultra rápido usando Reflection.
/// Límite: No maneja anidamiento profundo ni formatos complejos personalizados más allá del tipo básico.
/// Si se requiere exportar DataTables complejos o gráficos, migrar a ClosedXML/EPPlus.
/// </summary>
public static class CsvExporter
{
    public static string GenerateCsv<T>(IEnumerable<T> items)
    {
        if (items == null || !items.Any()) return string.Empty;

        var properties = typeof(T).GetProperties();
        var sb = new StringBuilder();

        // Header
        var headers = properties.Select(p => EscapeCsv(p.Name));
        sb.AppendLine(string.Join(",", headers));

        // Data
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var val = p.GetValue(item);
                if (val is DateTime dt) return EscapeCsv(dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                if (val is DateTimeOffset dto) return EscapeCsv(dto.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                return EscapeCsv(val?.ToString() ?? string.Empty);
            });

            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return string.Empty;
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
