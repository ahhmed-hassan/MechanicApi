using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicDomain.WorkOrders.Billing
{
    public enum InvoiceStatus
    {
        Unpaid = 0,
        Paid = 1,
        Refunded = 2
    }
}
