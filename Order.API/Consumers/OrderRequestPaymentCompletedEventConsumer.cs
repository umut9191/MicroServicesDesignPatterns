using MassTransit;
using Order.API.Models;
using Shared.Interfaces;

namespace Order.API.Consumers
{
    public class OrderRequestPaymentCompletedEventConsumer : IConsumer<IOrderRequestPaymentCompletedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<OrderRequestPaymentCompletedEventConsumer> _logger;

        public OrderRequestPaymentCompletedEventConsumer(AppDbContext appDbContext, ILogger<OrderRequestPaymentCompletedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrderRequestPaymentCompletedEvent> context)
        {
            var order = await _appDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Completed;
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
