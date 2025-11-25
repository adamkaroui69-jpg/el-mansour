// El Mansour Syndic Manager - Excel Export Implementation
// This file contains the complete Excel generation code using ClosedXML
// To use: Install-Package ClosedXML
// Then uncomment the code in ReportingService.cs and replace with this implementation

/*
using ClosedXML.Excel;

namespace ElMansourSyndicManager.Infrastructure.Services;

public partial class ReportingService
{
    /// <summary>
    /// Generates Excel for monthly report using ClosedXML
    /// </summary>
    private async Task<byte[]> GenerateMonthlyReportExcelAsync(MonthlyReportDto report)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Rapport Mensuel");

            // Title
            worksheet.Cell(1, 1).Value = "Résidence El Mansour - Rapport Financier Mensuel";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 4).Merge();

            worksheet.Cell(2, 1).Value = FormatMonth(report.Month.ToString("yyyy-MM"));
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            worksheet.Cell(2, 1).Style.Font.FontSize = 14;

            int row = 4;

            // Summary
            worksheet.Cell(row, 1).Value = "Résumé Financier";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 12;
            row++;

            worksheet.Cell(row, 1).Value = "Total Collecté:";
            worksheet.Cell(row, 2).Value = report.TotalCollected;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
            row++;

            worksheet.Cell(row, 1).Value = "Total Dépensé:";
            worksheet.Cell(row, 2).Value = report.TotalSpent;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
            row++;

            worksheet.Cell(row, 1).Value = "Solde:";
            worksheet.Cell(row, 2).Value = report.Balance;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
            worksheet.Cell(row, 2).Style.Font.FontColor = report.Balance >= 0 ? XLColor.Green : XLColor.Red;
            row += 2;

            // Statistics
            worksheet.Cell(row, 1).Value = "Statistiques";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            worksheet.Cell(row, 1).Value = "Maisons Payées:";
            worksheet.Cell(row, 2).Value = report.PaidHousesCount;
            row++;

            worksheet.Cell(row, 1).Value = "Maisons Non Payées:";
            worksheet.Cell(row, 2).Value = report.UnpaidHousesCount;
            row++;

            worksheet.Cell(row, 1).Value = "Taux de Collecte:";
            worksheet.Cell(row, 2).Value = report.CollectionRate / 100;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "0.0%";
            row++;

            worksheet.Cell(row, 1).Value = "Délai Moyen de Paiement:";
            worksheet.Cell(row, 2).Value = report.AveragePaymentDelay;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "0.0 \"jours\"";
            row += 2;

            // Building Breakdown
            if (report.BuildingBreakdown.Any())
            {
                worksheet.Cell(row, 1).Value = "Répartition par Bâtiment";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                row++;

                worksheet.Cell(row, 1).Value = "Bâtiment";
                worksheet.Cell(row, 2).Value = "Montant Collecté";
                worksheet.Range(row, 1, row, 2).Style.Font.Bold = true;
                row++;

                foreach (var building in report.BuildingBreakdown.OrderBy(b => b.Key))
                {
                    worksheet.Cell(row, 1).Value = $"Bâtiment {building.Key}";
                    worksheet.Cell(row, 2).Value = building.Value;
                    worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
                    row++;
                }
                row++;
            }

            // Payments Sheet
            var paymentsSheet = workbook.Worksheets.Add("Paiements");
            paymentsSheet.Cell(1, 1).Value = "Code Maison";
            paymentsSheet.Cell(1, 2).Value = "Montant";
            paymentsSheet.Cell(1, 3).Value = "Date";
            paymentsSheet.Cell(1, 4).Value = "Statut";
            paymentsSheet.Range(1, 1, 1, 4).Style.Font.Bold = true;

            int paymentRow = 2;
            foreach (var payment in report.Payments.OrderBy(p => p.HouseCode))
            {
                paymentsSheet.Cell(paymentRow, 1).Value = payment.HouseCode;
                paymentsSheet.Cell(paymentRow, 2).Value = payment.Amount;
                paymentsSheet.Cell(paymentRow, 2).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
                paymentsSheet.Cell(paymentRow, 3).Value = payment.PaymentDate;
                paymentsSheet.Cell(paymentRow, 3).Style.NumberFormat.Format = "dd/mm/yyyy";
                paymentsSheet.Cell(paymentRow, 4).Value = payment.Status;
                paymentRow++;
            }

            // Expenses Sheet
            var expensesSheet = workbook.Worksheets.Add("Dépenses");
            expensesSheet.Cell(1, 1).Value = "Description";
            expensesSheet.Cell(1, 2).Value = "Catégorie";
            expensesSheet.Cell(1, 3).Value = "Montant";
            expensesSheet.Cell(1, 4).Value = "Date";
            expensesSheet.Range(1, 1, 1, 4).Style.Font.Bold = true;

            int expenseRow = 2;
            foreach (var expense in report.Expenses.OrderBy(e => e.ExpenseDate))
            {
                expensesSheet.Cell(expenseRow, 1).Value = expense.Description;
                expensesSheet.Cell(expenseRow, 2).Value = expense.Category;
                expensesSheet.Cell(expenseRow, 3).Value = expense.Amount;
                expensesSheet.Cell(expenseRow, 3).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
                expensesSheet.Cell(expenseRow, 4).Value = expense.ExpenseDate;
                expensesSheet.Cell(expenseRow, 4).Style.NumberFormat.Format = "dd/mm/yyyy";
                expenseRow++;
            }

            // Unpaid Houses Sheet
            if (report.UnpaidHouses.Any())
            {
                var unpaidSheet = workbook.Worksheets.Add("Maisons Non Payées");
                unpaidSheet.Cell(1, 1).Value = "Code Maison";
                unpaidSheet.Cell(1, 2).Value = "Propriétaire";
                unpaidSheet.Cell(1, 3).Value = "Montant";
                unpaidSheet.Cell(1, 4).Value = "Jours de Retard";
                unpaidSheet.Range(1, 1, 1, 4).Style.Font.Bold = true;

                int unpaidRow = 2;
                foreach (var house in report.UnpaidHouses.OrderBy(h => h.DaysOverdue))
                {
                    unpaidSheet.Cell(unpaidRow, 1).Value = house.HouseCode;
                    unpaidSheet.Cell(unpaidRow, 2).Value = house.OwnerName;
                    unpaidSheet.Cell(unpaidRow, 3).Value = house.MonthlyAmount;
                    unpaidSheet.Cell(unpaidRow, 3).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
                    unpaidSheet.Cell(unpaidRow, 4).Value = house.DaysOverdue;
                    unpaidRow++;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        });
    }

    /// <summary>
    /// Generates Excel for yearly report using ClosedXML
    /// </summary>
    private async Task<byte[]> GenerateYearlyReportExcelAsync(YearlyReportDto report)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();

            // Summary Sheet
            var summarySheet = workbook.Worksheets.Add("Résumé Annuel");
            summarySheet.Cell(1, 1).Value = $"Résidence El Mansour - Rapport Financier Annuel {report.Year}";
            summarySheet.Cell(1, 1).Style.Font.Bold = true;
            summarySheet.Cell(1, 1).Style.Font.FontSize = 16;
            summarySheet.Range(1, 1, 1, 4).Merge();

            int row = 3;
            summarySheet.Cell(row, 1).Value = "Total Collecté:";
            summarySheet.Cell(row, 2).Value = report.TotalCollected;
            summarySheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
            row++;

            summarySheet.Cell(row, 1).Value = "Total Dépensé:";
            summarySheet.Cell(row, 2).Value = report.TotalSpent;
            summarySheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
            row++;

            summarySheet.Cell(row, 1).Value = "Solde Annuel:";
            summarySheet.Cell(row, 2).Value = report.Balance;
            summarySheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 \"MAD\"";
            row += 2;

            // Monthly Breakdown
            summarySheet.Cell(row, 1).Value = "Répartition Mensuelle";
            summarySheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            summarySheet.Cell(row, 1).Value = "Mois";
            summarySheet.Cell(row, 2).Value = "Collecté";
            summarySheet.Cell(row, 3).Value = "Dépensé";
    }
}
*/

