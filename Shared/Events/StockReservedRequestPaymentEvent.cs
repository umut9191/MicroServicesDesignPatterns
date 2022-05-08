﻿using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class StockReservedRequestPaymentEvent : IStockReservedRequestPaymentEvent
    {
        public StockReservedRequestPaymentEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public PaymentMessage Payment { get; set; }
        public int BuyerId { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }

        public Guid CorrelationId { get; }
    }
}
