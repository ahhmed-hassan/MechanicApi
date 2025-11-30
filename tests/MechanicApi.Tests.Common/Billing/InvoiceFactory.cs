using ErrorOr;
using MechanicDomain.WorkOrders.Billing;

namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceFactory
{
    public static ErrorOr<Invoice> CreateInvoice(
        Guid? id = null,
        Guid? workOrderId = null,
        List<InvoiceLineItem>? items = null,
        decimal discount = 0,
        decimal taxAmount = 0,
        TimeProvider? timeProvider = null) => Invoice.Create(id ?? Guid.NewGuid(),
            workOrderId ?? Guid.NewGuid(),
            items ?? [InvoiceLineItem.Create(Guid.NewGuid(), 1, "Oil Change", 2, 50).Value],
            discount  ,
            taxAmount ,
            timeProvider ?? TimeProvider.System);
}