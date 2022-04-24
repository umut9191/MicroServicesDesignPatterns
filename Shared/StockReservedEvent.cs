using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class StockReservedEvent
    {
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public PaymentMessage payment { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }
}
