using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MechanicDomain.WorkOrders.Billing;

public sealed class InvoiceLineItem
{

    public Guid InvoiceId { get; }
    public int LineNumber { get; }
    public string Description { get; } = string.Empty;
    public decimal UnitPrice { get; }

    public int Quantity { get; }
    public decimal LineTotalPrice => Quantity * UnitPrice;

    private InvoiceLineItem() { }
    private InvoiceLineItem(
        Guid invoiceId,
        int lineNumber,
        string description,
        int quantity,
        decimal unitPrice)
    {
        InvoiceId = invoiceId;
        LineNumber = lineNumber;
        Description = description;
        UnitPrice = unitPrice;
    }
    public static ErrorOr<InvoiceLineItem> Create (
        Guid invoiceId,
        int lineNumber,
        string description,
        int quantity,
        decimal unitPrice
        )
    {
        if (invoiceId == Guid.Empty) return InvoiceLineItemErrors.InvoiceIdRequired;
        if (lineNumber <= 0) return InvoiceLineItemErrors.LineNumberInvalid;
        if (string.IsNullOrEmpty(description)) return InvoiceLineItemErrors.DescriptionRequired;
        if (quantity <= 0) return InvoiceLineItemErrors.QuantityInvalid;
        if (unitPrice <= 0) return InvoiceLineItemErrors.UnitPriceInvalid; 

        return new InvoiceLineItem(invoiceId, lineNumber, description, quantity, unitPrice);

    }

}

