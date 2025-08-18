using MechanicDomain.WorkOrders.Billing;

namespace MechanicApplication.Common.Interfaces;

public interface IInvoicePdfGenerator
{
    byte[] Generatre(Invoice invoice);
}
