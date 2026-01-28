namespace CafePos.Application.Models;

public class OrderDetailDto
{
    public Guid OrderId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public IList<OrderLineDto> Lines { get; set; } = new List<OrderLineDto>();
}

public class OrderLineDto
{
    public Guid OrderLineId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal LineTotal { get; set; }
    public string? LineNote { get; set; }
}

public class HeldOrderDto
{
    public Guid OrderId { get; set; }
    public string? HeldName { get; set; }
    public DateTime CreatedAtLocal { get; set; }
    public decimal Total { get; set; }
}
