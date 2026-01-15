using System.Collections.Generic;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services
{
    public interface IExportService
    {
        /// <summary>
        /// Exporte une liste de données vers un fichier Excel (XLSX).
        /// </summary>
        /// <typeparam name="T">Type des objets à exporter</typeparam>
        /// <param name="data">Liste des objets</param>
        /// <param name="sheetName">Nom de la feuille Excel</param>
        /// <returns>Tableau d'octets du fichier Excel</returns>
        byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Data");

        /// <summary>
        /// Exporte une liste de données vers un fichier CSV.
        /// </summary>
        /// <typeparam name="T">Type des objets à exporter</typeparam>
        /// <param name="data">Liste des objets</param>
        /// <returns>Tableau d'octets du fichier CSV</returns>
        byte[] ExportToCsv<T>(IEnumerable<T> data);
    }
}
