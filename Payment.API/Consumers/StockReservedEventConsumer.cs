using MassTransit;
using Shared;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(ILogger<StockReservedEventConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var balance = 3000m;
            if (balance>=context.Message.payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.payment.TotalPrice} TL was withdraw from credit card" +
                    $"for userId ={context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentCompletedEvent() { BuyerId = context.Message.BuyerId, 
                OrderId = context.Message.OrderId});
            }
            else
            {
                _logger.LogInformation($"{context.Message.payment.TotalPrice} TL was not withdraw from credit card" +
                    $"for user Id= {context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentFailedEvent()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems,
                    Message = $"{context.Message.payment.TotalPrice} TL was not withdraw from credit card" +
                    $"for user Id= {context.Message.BuyerId}"
                });
            }
        }
    }
}
