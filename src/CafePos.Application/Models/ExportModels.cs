namespace CafePos.Application.Models;

public record OrderExportRow(
    string ReceiptNumber,
    DateTime? CompletedAtLocal,
    string Status,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    decimal TotalPaid
);

public record OrderLineExportRow(
    string ReceiptNumber,
    DateTime? CompletedAtLocal,
    string LineDescription,
    decimal Qty,
    decimal LineTotal,
    decimal TaxAmount
);
