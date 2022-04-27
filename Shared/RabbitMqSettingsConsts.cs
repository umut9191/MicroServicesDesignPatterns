using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class RabbitMqSettingsConsts
    {
        public const string StockOReservedEventQueueName = "stock-reserved-queue";
        public const string StockNotReservedEventQueueName = "Order-stock-not--reserved-queue";
        public const string StockOrderCreatedEventQueueName = "stock-order-created-queue";
        public const string OrderPaymentCompletedEventQueueName = "order-payment-completed-queue";
        public const string OrderPaymentFailedEventQueueName = "order-payment-failed-queue";

    }
}
