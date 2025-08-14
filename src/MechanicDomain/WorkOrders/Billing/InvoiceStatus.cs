using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicDomain.WorkOrders.Billing
{
    //TODO: Refctor this so Paid state also enapsulates when it was paid. 
    public enum InvoiceStatus
    {
        Unpaid = 0,
        Paid = 1,
        Refunded = 2
    }
}
