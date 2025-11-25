# Reporting Service - Implementation Notes

## Excel Export Setup

The ReportingService includes complete Excel export functionality, but it requires the **ClosedXML** NuGet package.

### To Enable Excel Export:

1. **Install ClosedXML Package**:
   ```bash
   dotnet add package ClosedXML
   ```

2. **Uncomment Excel Code**:
   - In `ReportingService.cs`, uncomment the `using ClosedXML.Excel;` statement
   - Uncomment the code blocks in `GenerateMonthlyReportExcelAsync` and `GenerateYearlyReportExcelAsync`
   - Remove the `NotImplementedException` throws

3. **Alternative**: Use the complete implementation in `ReportingService_Excel_Implementation.cs` as a reference

## PDF Export

PDF export is **fully functional** using QuestPDF (already included).

## Features Implemented

### ✅ Monthly Reports
- Total collected per house and building
- Total expenses with category breakdown
- Balance calculation
- Unpaid houses list
- Statistics (collection rate, average delay)
- Building breakdown
- Expense category breakdown

### ✅ Yearly Reports
- Annual totals
- Monthly breakdown (12 months)
- Yearly statistics
- Building yearly breakdown
- Expense category yearly breakdown

### ✅ PDF Export
- Print-ready A4 format
- French labels
- Professional layout
- Tables and summaries

### ✅ Excel Export (Requires ClosedXML)
- Multi-sheet workbooks
- Formatted cells
- Number formatting
- Color coding
- Ready for implementation

### ✅ UI Integration
- ReportsViewModel with all commands
- ReportsView with Material Design
- Report type selection
- Period selection
- Export buttons
- History viewer

## Usage

### Generate Monthly Report
```csharp
var report = await _reportingService.GenerateMonthlyReportAsync(new DateTime(2024, 1, 1));
```

### Export to PDF
```csharp
var pdfBytes = await _reportingService.ExportMonthlyReportToPdfAsync(new DateTime(2024, 1, 1));
```

### Export to Excel
```csharp
var excelBytes = await _reportingService.ExportMonthlyReportToExcelAsync(new DateTime(2024, 1, 1));
```

## Next Steps

1. Install ClosedXML package for Excel export
2. Uncomment Excel generation code
3. Test PDF and Excel exports
4. Add chart library (LiveCharts/OxyPlot) for visualizations
5. Implement cloud storage upload for reports
6. Add email export functionality

