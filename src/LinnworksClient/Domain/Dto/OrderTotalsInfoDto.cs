namespace LinnworksClient.Domain.Dto;

public sealed class OrderTotalsInfoDto
{
    public double Subtotal { get; set; }

    public double TotalCharge { get; set; }

    public double PostageCost { get; set; }

    public double PostageCostExTax { get; set; }
}
