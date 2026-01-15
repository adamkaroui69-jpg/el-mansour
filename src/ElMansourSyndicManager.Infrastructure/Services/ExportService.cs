using ClosedXML.Excel;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ElMansourSyndicManager.Infrastructure.Services
{
    public class ExportService : IExportService
    {
        public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Data")
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);
                
                // Charger les données à partir de A1. InsertTable crée automatiquement les headers.
                // Note: Si la liste est vide, cela peut throw. A gérer.
                if (data != null && data.Any())
                {
                    worksheet.Cell(1, 1).InsertTable(data);
                }
                else
                {
                    // Si vide, mettre juste les headers manuellement
                    var props = typeof(T).GetProperties();
                    for (int i = 0; i < props.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = props[i].Name;
                    }
                }
                
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] ExportToCsv<T>(IEnumerable<T> data)
        {
            var sb = new StringBuilder();
            // Utiliser ; comme séparateur (format Excel FR souvent)
            var separator = ";"; 
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // Header
            sb.AppendLine(string.Join(separator, properties.Select(p => $"\"{p.Name}\"")));

            // Rows
            if (data != null)
            {
                foreach (var item in data)
                {
                    var values = properties.Select(p => 
                    {
                        var val = p.GetValue(item, null);
                        var strVal = val?.ToString() ?? "";
                        // Echapper les guillemets et entourer de guillemets
                        return $"\"{strVal.Replace("\"", "\"\"")}\"";
                    });
                    sb.AppendLine(string.Join(separator, values));
                }
            }

            // UTF8 avec BOM pour Excel
            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }
    }
}
