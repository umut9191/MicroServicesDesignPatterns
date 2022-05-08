using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IStockReservedEvent> StockReservedEvent { get; set; }
        public Event<IStockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<IPaymentSuccessedEvent> PaymentCompletedEvent { get; set; }
        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public State PaymentCompleted { get; private set; }
        public State StockNotReserved { get; private set; }
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);
            //OrderCreatedRequestEvent oluşunca aynısı db de var ise tekrar oluşmasın;
            //eğer yok ise yeni bir satır oluştur ve oluşturduğun OrderStateInstance in CorrelationId ne yeni bir değer ata.
            Event(() => OrderCreatedRequestEvent, y => y.CorrelateBy<int>(x => x.OrderId, z => z.Message.OrderId)
            .SelectId(context => Guid.NewGuid()));

            //when StockReservedEvent occure use CorrelationId on db
            Event(() => StockReservedEvent, x => x.CorrelateById(y=>y.Message.CorrelationId));

            //when PaymentCompletedEvent occured use CorrelationId on db
            Event(() => PaymentCompletedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
            //when StockNotReservedEvent occured  use CorrelationId on db
            Event(() => StockNotReservedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));




            //instanceler veritabanındaki Data lar ise Eventteki 
            Initially(When(OrderCreatedRequestEvent).Then(context =>
            {
                context.Instance.BuyerId = context.Data.BuyerId;
                context.Instance.OrderId = context.Data.OrderId;
                context.Instance.CreatedDate = DateTime.Now;
                context.Instance.CardName = context.Data.Payment.CardName;
                context.Instance.CardNumber = context.Data.Payment.CardNumber;
                context.Instance.CVV = context.Data.Payment.CVV;
                context.Instance.Expiration = context.Data.Payment.Expiration;
                context.Instance.TotalPrice = context.Data.Payment.TotalPrice;

            }).Then(context =>
            {
                Console.WriteLine($"OrderCreatedRequestEvent before : {context.Instance}");//toString() override methodu çalışacak.
            }).Publish(context => new OrderCreatedEvent(context.Instance.CorrelationId) { OrderItems = context.Data.OrderItems })
            .TransitionTo(OrderCreated)//initialdan order created e geçilsin
            .Then(context =>
            {
                Console.WriteLine($"OrderCreatedRequestEvent after : {context.Instance}");//toString() override methodu çalışacak.
            })
            );
            //StockReservedEvent
            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMqSettingsConsts.StockReservedRequestPaymentEventQueueName}"),
                context => new StockReservedRequestPaymentEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems,
                    Payment = new PaymentMessage()
                    {
                        CardName = context.Instance.CardName,
                        CardNumber = context.Instance.CardNumber,
                        CVV = context.Instance.CVV,
                        Expiration = context.Instance.Expiration,
                        TotalPrice = context.Instance.TotalPrice
                    },
                    BuyerId = context.Instance.BuyerId
                })
                 .Then(context =>
                 {
                     Console.WriteLine($"StockReservedEvent after : {context.Instance}");//toString() override methodu çalışacak.
                 }),
                When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Publish(context=> new OrderRequestFailedEvent() 
                { 
                    OrderId = context.Instance.OrderId,
                    Reason =  context.Data.reason
                
                }).Then(context =>
                {
                    Console.WriteLine($"StockNotReservedEvent after : {context.Instance}");//toString() override methodu çalışacak.
                })
                );

            //PaymentCompletedEvent
            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
             .Publish(context => new OrderRequestPaymentCompletedEvent() { OrderId = context.Instance.OrderId})
                 .Then(context =>
                 {
                     Console.WriteLine($"OrderRequestPaymentCompletedEvent after : {context.Instance}");//toString() override methodu çalışacak.
                 }).Finalize().Then(context =>
                 {
                     Console.WriteLine($"OrderRequestPaymentCompletedEvent after Finalize : {context.Instance}");//toString() override methodu çalışacak.
                 })
                );
        }
    }
}
