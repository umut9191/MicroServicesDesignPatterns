using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class OrderRequestPaymentCompletedEvent : IOrderRequestPaymentCompletedEvent
    {
        public int OrderId { get; set; }
    }
}
