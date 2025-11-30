using MechanicDomain.WorkOrders.Billing;
using ErrorOr;

namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceLineItemFactory
{
    public static ErrorOr<InvoiceLineItem> CreateInvoiceLineItem(
        Guid? id = null,
        int? lineNumber = null,
        string? description = null,
        int? quantity = null,
        decimal? unitPrice = null)
    {
        return InvoiceLineItem.Create(
            id ?? Guid.NewGuid(),
            lineNumber ?? 1,
            description ?? "some invoice line",
            quantity ?? 1,
            unitPrice ?? 100m);
    }
}