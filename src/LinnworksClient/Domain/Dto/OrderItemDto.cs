namespace LinnworksClient.Domain.Dto;

public sealed class OrderItemDto
{
    public string Title { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string SKU { get; set; } = null!;

    public int Quantity { get; set; }

    public double CostIncTax { get; set; }
}
