using LinnworksClient.Domain.Dto;

namespace LinnworksClient.Domain;

public interface ILinnworksClient
{
    Task<int> GetInventoryItemsCount(CancellationToken cancellationToken);

    Task<IEnumerable<InventoryItemDto>> GetStockItemsFull(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OpenOrderDto>> GetOpenOrders(CancellationToken cancellationToken);

    Task SetStockLevel(IEnumerable<SetStockLevelDto> dtos, CancellationToken cancellationToken);

    Task UpdateInventoryItemPrices(IEnumerable<StockItemPriceDto> dtos, CancellationToken cancellationToken);

    Task SetOrderShippingInfo(
        Guid orderId,
        SetOrderShippingInfoDto info,
        CancellationToken cancellationToken);
}
