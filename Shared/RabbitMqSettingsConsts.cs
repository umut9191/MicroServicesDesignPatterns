using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class RabbitMqSettingsConsts
    {
        public const string OrderSaga = "order-saga-queue";
        public const string StockReservedRequestPaymentEventQueueName = "stock-reserved-request-payment-event-queue";
        public const String OrderRequestPaymentCompletedEventQueueName = "order-request-payment-completed-event-queue";
        //------//
        public const string StockOReservedEventQueueName = "stock-reserved-queue";
        public const string StockNotReservedEventQueueName = "Order-stock-not--reserved-queue";
        public const string StockOrderCreatedEventQueueName = "stock-order-created-queue";
       // public const string OrderPaymentCompletedEventQueueName = "order-payment-completed-queue";
        public const string OrderPaymentFailedEventQueueName = "order-payment-failed-queue";
        public const string StockPaymentFailedEventQueueName = "stock-payment-failed-queue";



    }
}
