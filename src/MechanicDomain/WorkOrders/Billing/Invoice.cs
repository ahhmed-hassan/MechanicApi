using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;
using MechanicDomain.Abstractions;
namespace MechanicDomain.WorkOrders.Billing;

public sealed class Invoice : AuditableEntity
{
    public Guid WorkOrderId { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; }
    public decimal Subtotal => LineItems.Sum(x => x.LineTotalPrice);
    public decimal Total => Subtotal - DiscountAmount + TaxAmount;

    public DateTimeOffset? PaidAt { get; private set; }

    public WorkOrder? WorkOrder { get; set; }

    public IReadOnlyList<InvoiceLineItem> LineItems { get; } = new List<InvoiceLineItem>();

    public InvoiceStatus Status { get; private set; }

    private Invoice()
    { }

    private Invoice(
        Guid id,
        Guid workOrderId,
        DateTimeOffset issuedAt,
        List<InvoiceLineItem> lineItems,
        decimal discountAmount,
        decimal taxAmount)
        : base(id)
    {
        WorkOrderId = workOrderId;
        IssuedAtUtc = issuedAt;
        DiscountAmount = discountAmount;
        Status = InvoiceStatus.Unpaid;
        TaxAmount = taxAmount;
        LineItems = lineItems;
    }

    public static ErrorOr<Invoice> Create(
       Guid id,
       Guid workOrderId,
       List<InvoiceLineItem> items,
       decimal discountAmount,
       decimal taxAmount,
       TimeProvider datetime)
    {
        if (workOrderId.Equals(Guid.Empty)) return InvoiceErrors.WorkOrderIdInvalid;
        if (!items?.Any() ?? false) return InvoiceErrors.LineItemsEmpty;
        return new Invoice(id, workOrderId, datetime.GetUtcNow(), items, discountAmount, taxAmount);
    }

    public ErrorOr<Updated> ApplyDiscount(decimal discountAmount)
    {
        if (Status != InvoiceStatus.Unpaid) return InvoiceErrors.InvoiceLocked;

        if (discountAmount < 0) return InvoiceErrors.DiscountExceedsSubtotal;
        if (discountAmount > Subtotal) return InvoiceErrors.DiscountExceedsSubtotal;
        DiscountAmount = discountAmount;
        return Result.Updated;
    }

    public ErrorOr<Updated> MarkAsPaid(TimeProvider timeProvider)
    {
        if (Status != InvoiceStatus.Unpaid) return InvoiceErrors.InvoiceLocked;
        Status = InvoiceStatus.Paid;
        PaidAt = timeProvider.GetUtcNow();
        return Result.Updated;
    }
}

