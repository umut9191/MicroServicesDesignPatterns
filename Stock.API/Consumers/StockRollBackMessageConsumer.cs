using MassTransit;
using Stock.API.Models;
using Shared.Messages;
using Microsoft.EntityFrameworkCore;

namespace Stock.API.Consumers
{
    public class StockRollBackMessageConsumer : IConsumer<IStockRollBackMessage>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<StockRollBackMessageConsumer> _logger;

        public StockRollBackMessageConsumer(AppDbContext appDbContext, ILogger<StockRollBackMessageConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IStockRollBackMessage> context)
        {
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _appDbContext.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);
                    if (stock != null)
                    {
                        stock.Count += item.Count;
                        await _appDbContext.SaveChangesAsync();
                    }
                }
                _logger.LogInformation($"Stock was released for Order Id {context.Message.OrderId}");
            }
        }
    }
}
