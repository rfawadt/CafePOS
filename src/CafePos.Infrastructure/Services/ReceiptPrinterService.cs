using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using CafePos.Application.Interfaces;
using CafePos.Application.Models;

namespace CafePos.Infrastructure.Services;

public class ReceiptPrinterService : IPrinterService
{
    private readonly string? _printerName;

    public ReceiptPrinterService(string? printerName)
    {
        _printerName = printerName;
    }

    public Task PrintReceiptAsync(ReceiptData data, CancellationToken cancellationToken = default)
    {
        var receiptText = BuildReceiptText(data);

        try
        {
            var printDocument = new PrintDocument();
            if (!string.IsNullOrWhiteSpace(_printerName))
            {
                printDocument.PrinterSettings.PrinterName = _printerName;
            }

            printDocument.PrintPage += (_, args) =>
            {
                using var font = new Font("Consolas", 10);
                args.Graphics.DrawString(receiptText, font, Brushes.Black, new RectangleF(0, 0, args.PageBounds.Width, args.PageBounds.Height));
                args.HasMorePages = false;
            };

            printDocument.Print();
        }
        catch
        {
            WriteReceiptToFile(receiptText);
        }

        return Task.CompletedTask;
    }

    private static string BuildReceiptText(ReceiptData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine(data.StoreName);
        if (!string.IsNullOrWhiteSpace(data.StoreAddress)) sb.AppendLine(data.StoreAddress);
        if (!string.IsNullOrWhiteSpace(data.StorePhone)) sb.AppendLine(data.StorePhone);
        sb.AppendLine($"Receipt: {data.ReceiptNumber}");
        sb.AppendLine($"Date: {data.PrintedAtLocal:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Terminal: {data.TerminalName}  Cashier: {data.CashierName}");
        sb.AppendLine(new string('-', 32));

        foreach (var line in data.Lines)
        {
            sb.AppendLine($"{line.Qty} x {line.Description}");
            sb.AppendLine($"  {line.LineTotal:C}");
            foreach (var mod in line.Modifiers)
            {
                sb.AppendLine($"   + {mod.Name} {mod.PriceDelta:C}");
            }
            if (!string.IsNullOrWhiteSpace(line.LineNote))
            {
                sb.AppendLine($"   Note: {line.LineNote}");
            }
        }

        sb.AppendLine(new string('-', 32));
        sb.AppendLine($"Subtotal: {data.Subtotal:C}");
        sb.AppendLine($"Tax: {data.TaxTotal:C}");
        sb.AppendLine($"Total: {data.Total:C}");
        sb.AppendLine(new string('-', 32));

        foreach (var payment in data.Payments)
        {
            var label = payment.IsRefund ? "Refund" : "Payment";
            sb.AppendLine($"{label} {payment.Method}: {payment.Amount:C}");
        }

        if (data.ChangeDue > 0)
        {
            sb.AppendLine($"Change: {data.ChangeDue:C}");
        }

        sb.AppendLine(new string('-', 32));
        if (!string.IsNullOrWhiteSpace(data.Footer))
        {
            sb.AppendLine(data.Footer);
        }
        return sb.ToString();
    }

    private static void WriteReceiptToFile(string receiptText)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "receipts");
        Directory.CreateDirectory(directory);
        var fileName = Path.Combine(directory, $"receipt_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        File.WriteAllText(fileName, receiptText);
    }
}
