namespace CafePos.Domain.Enums;

public enum ModifierSelectionType
{
    Single = 0,
    Multiple = 1
}

public enum TaxRoundingMode
{
    LineLevel = 0,
    InvoiceLevel = 1
}

public enum OrderStatus
{
    Open = 0,
    Held = 1,
    Completed = 2,
    Voided = 3,
    Refunded = 4,
    PartiallyRefunded = 5
}

public enum OrderType
{
    Takeaway = 0,
    DineIn = 1,
    Delivery = 2
}

public enum PaymentMethodType
{
    Cash = 0,
    External = 1
}

public enum ShiftStatus
{
    Open = 0,
    Closed = 1
}

public enum CashDrawerEventType
{
    PayIn = 0,
    PayOut = 1,
    CashDrop = 2,
    NoSale = 3
}
