using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Application.Models;
using CafePos.Application.Services;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly ReportService _reportService;
    private readonly ReportExportService _exportService;
    private readonly NavigationService _navigation;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private IList<DailySummaryDto> _dailySummaries = new List<DailySummaryDto>();

    [ObservableProperty]
    private MonthlySummaryDto? _monthlySummary;

    [ObservableProperty]
    private string _statusMessage = "";

    public ReportsViewModel(ReportService reportService, ReportExportService exportService, NavigationService navigation)
    {
        _reportService = reportService;
        _exportService = exportService;
        _navigation = navigation;
        _ = RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        DailySummaries = (await _reportService.GetDailySummaryAsync(DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate))).ToList();
        MonthlySummary = await _reportService.GetMonthlySummaryAsync(DateTime.Now.Year, DateTime.Now.Month);
    }

    [RelayCommand]
    private void Back()
    {
        _navigation.NavigateTo<MainPosViewModel>();
    }

    [RelayCommand]
    private void ExportSummary()
    {
        StatusMessage = "";
        var rows = DailySummaries;
        if (rows.Count == 0)
        {
            StatusMessage = "No daily summaries to export.";
            return;
        }

        var header = "Date,GrossSales,NetSales,TaxTotal,OrdersCount";
        var lines = new List<string> { header };
        lines.AddRange(rows.Select(r => $"{r.Date},{r.GrossSales},{r.NetSales},{r.TaxTotal},{r.OrdersCount}"));
        var path = CsvExporter.Export(lines, line => line, "daily_summary");
        StatusMessage = $"Exported to {path}";
    }

    [RelayCommand]
    private async Task ExportOrdersAsync()
    {
        StatusMessage = "";
        var orders = await _exportService.GetOrdersAsync(DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate));
        if (orders.Count == 0)
        {
            StatusMessage = "No orders to export.";
            return;
        }

        var header = "ReceiptNumber,CompletedAt,Status,Subtotal,TaxTotal,Total,TotalPaid";
        var lines = new List<string> { header };
        lines.AddRange(orders.Select(o => $"{o.ReceiptNumber},{o.CompletedAtLocal:yyyy-MM-dd HH:mm},{o.Status},{o.Subtotal},{o.TaxTotal},{o.Total},{o.TotalPaid}"));
        var path = CsvExporter.Export(lines, line => line, "orders");
        StatusMessage = $"Exported to {path}";
    }
}
