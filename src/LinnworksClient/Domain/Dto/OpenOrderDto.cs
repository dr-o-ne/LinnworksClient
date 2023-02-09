namespace LinnworksClient.Domain.Dto;

public sealed class OpenOrderDto
{
    public Guid OrderId { get; set; }

    public int NumOrderId { get; set; }

    public CustomerInfoDto CustomerInfo { get; set; } = null!;

    public OrderGeneralInfoDto GeneralInfo { get; set; } = null!;

    public OrderTotalsInfoDto TotalsInfo { get; set; } = null!;

    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}
