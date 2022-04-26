using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<PaymentCompletedEventConsumer> _logger;

        public PaymentCompletedEventConsumer(AppDbContext appDbContext, ILogger<PaymentCompletedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
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
