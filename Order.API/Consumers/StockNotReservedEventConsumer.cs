using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<StockNotReservedEvent> _logger;

        public StockNotReservedEventConsumer(AppDbContext appDbContext, ILogger<StockNotReservedEvent> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            var order = await _appDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Fail;
                order.FailMessage = context.Message.message??"";
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
