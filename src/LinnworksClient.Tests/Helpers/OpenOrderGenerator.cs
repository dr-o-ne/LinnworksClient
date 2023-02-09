using System;
using System.Collections.Generic;
using LinnworksClient.Domain.Dto;

namespace LinnworksClient.Tests.Helpers;

internal static class OpenOrderGenerator
{
    public static List<OpenOrderDto> GenerateOrders(int count, int? from = null)
    {
        var result = new List<OpenOrderDto>();
        for (var i = 0; i < count; i++)
        {
            result.Add(GenerateOrder(i + (from ?? 0) + 1));
        }

        return result;
    }

    private static OpenOrderDto GenerateOrder(int orderId) =>
        new()
        {
            OrderId = Guid.NewGuid(),
            NumOrderId = orderId,
            GeneralInfo = new OrderGeneralInfoDto { NumItems = 1, ReferenceNum = "xxx" },
            TotalsInfo = new OrderTotalsInfoDto(),
            Items = new List<OrderItemDto>
            {
                new() { SKU = orderId.ToString().PadLeft(8, '0'), CostIncTax = 10, Quantity = 1 }
            }
        };
}
