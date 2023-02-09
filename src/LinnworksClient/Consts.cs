namespace LinnworksClient;

public static class Consts
{
    internal static class LinnworksUrl
    {
        public const string GetInventoryItemsCount = "/api/Inventory/GetInventoryItemsCount";

        public const string SetStockLevel = "/api/Stock/SetStockLevel";

        public const string UpdateInventoryItemPrices = "/api/Inventory/UpdateInventoryItemPrices";

        public const string GetOpenOrdersUrl = "/api/Orders/GetOpenOrders";

        public const string GetStockItemsFull = "/api/Stock/GetStockItemsFull";

        public const string SetOrderShippingInfo = "/api/Orders/SetOrderShippingInfo";
    }
}