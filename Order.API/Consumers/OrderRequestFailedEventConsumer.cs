using MassTransit;
using Order.API.Models;
using Shared.Interfaces;

namespace Order.API.Consumers
{
    public class OrderRequestFailedEventConsumer : IConsumer<IOrderRequestFailedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<OrderRequestFailedEventConsumer> _logger;

        public OrderRequestFailedEventConsumer(AppDbContext appDbContext, ILogger<OrderRequestFailedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IOrderRequestFailedEvent> context)
        {
            var order = await _appDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Fail;
                order.FailMessage = context.Message.Reason ?? "";
                await _appDbContext.SaveChangesAsync();
                _logger.LogInformation($"Order id = {context.Message.OrderId} status changed : {order.Status}");
            }
            else
            {
                _logger.LogInformation($"Order id = {context.Message.OrderId} not found");

            }
        }
    }
}
