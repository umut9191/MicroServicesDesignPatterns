﻿using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly AppDbContext _context;
        private ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndPoint;

        public OrderCreatedEventConsumer(AppDbContext context,
            ILogger<OrderCreatedEventConsumer> logger,
            ISendEndpointProvider sendEndpointProvider,
            IPublishEndpoint publishEndPoint)
        {
            _context = context;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndPoint = publishEndPoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();
            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(
                    await _context.Stocks.AnyAsync(
                        x => x.ProductId == item.ProductId
                    && x.Count > item.Count
                    ));
            }
            if (stockResult.All(x=>x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _context.Stocks.FirstOrDefaultAsync(x=>x.ProductId== item.ProductId);
                    if (stock != null)
                    {
                        stock.Count -= item.Count;
                    }
                    await _context.SaveChangesAsync();
                }
                _logger.LogInformation($"Stock was reserved for BuyerId: {context.Message.BuyerId }");
                var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettingsConsts.StockOReservedEventQueueName}"));
                StockReservedEvent stockReservedEvent = new StockReservedEvent
                {
                    payment = context.Message.Payment,
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems
                };
                await sendEndpoint.Send(stockReservedEvent);
            
            }
            else
            {
                await _publishEndPoint.Publish(new StockNotReservedEvent()             
                {
                  
                    OrderId=context.Message.OrderId,
                    message= "Not enough stock"
                
                });
                _logger.LogInformation($"Stock not enough for BuyerId: {context.Message.BuyerId }");

            }
        }
    }
}
