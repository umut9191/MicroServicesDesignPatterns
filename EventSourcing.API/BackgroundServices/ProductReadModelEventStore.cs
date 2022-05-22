using EventSourcing.API.EventStores;
using EventSourcing.API.Models;
using EventSourcing.Shared.Events;
using EventStore.ClientAPI;
using System.Text;
using System.Text.Json;

namespace EventSourcing.API.BackgroundServices
{
    public class ProductReadModelEventStore : BackgroundService
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly ILogger<ProductReadModelEventStore> _logger;
        //Scope den Singleton a erişilebilir fakat;
        //Singleton dan Scope a erişilemez ama erişmek için service provider kullanacağız.
        private readonly IServiceProvider _serviceProvider;

        public ProductReadModelEventStore(IEventStoreConnection eventStoreConnection, 
            ILogger<ProductReadModelEventStore> logger, IServiceProvider serviceProvider)
        {
            _eventStoreConnection = eventStoreConnection;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //uygulama başlayınca çalışan yerdir.
            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            //uygulama kapanınca çalışan yerdir.
            return base.StopAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //ilgili stream ın ilgili group una event eklendiğinde ya da beklemede olan eventler olduğunda buraya gelirler
            //ve aşağıda oluşturduğumuz EventAppeared isimli method çalışacak
            //autoAck :false eğer true olsaydı message brokker bizim bu mesajı alıp işlediğimizi kabul eder. ama false için bizden cevap bekler.
            await _eventStoreConnection.ConnectToPersistentSubscriptionAsync(
                ProductStream.StreamName, ProductStream.GroupName, EventAppeared,autoAck :false);
           //Uygulama ayağa kalktığı zaman çalışacak yerdir.
        }
        private async Task EventAppeared(EventStorePersistentSubscriptionBase arg1,ResolvedEvent arg2)
        {
            _logger.LogInformation("The Message processing ...");
            //gelen eventin tipi;
            var type = Type.GetType($"{Encoding.UTF8.GetString(arg2.Event.Metadata)}, EventSourcing.Shared");
            //gelen eventin datası;
            var eventData = Encoding.UTF8.GetString(arg2.Event.Data);

            var @event = JsonSerializer.Deserialize(eventData, type);

            using var scope  = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Product product = null;
            switch (@event)
            {
                case  ProductCreatedEvent productCreatedEvent:
                    product = new Product
                    {
                        Id = productCreatedEvent.Id,
                        Name = productCreatedEvent.Name,
                        Price = productCreatedEvent.Price,
                        Stock = productCreatedEvent.Stock,
                        UserId = productCreatedEvent.UserId,
                    };
                    context.Products.Add(product);
                    break;
                case ProductNameChangedEvent productNameChangedEvent:
                    product = context.Products.Find(productNameChangedEvent.Id);
                    if (product!=null)
                    {
                        product.Name = productNameChangedEvent.ChangedName;
                        //context.SaveChanges();
                    }
                    break;
                case ProductPriceChangedEvent productPriceChangedEvent:
                    product = context.Products.Find(productPriceChangedEvent.Id);
                    if (product != null)
                    {
                        product.Price = productPriceChangedEvent.ChangedPrice;                       
                    }
                    break;
                case ProductDeletedEvent productDeletedEvent:
                    product = context.Products.Find(productDeletedEvent.Id);
                    if (product!=null)
                    {
                        context.Products.Remove(product);
                    }
                    break;
            }
            await context.SaveChangesAsync();
            //autoAck :false demiştik bu yüzden mesajı aldığımız bilgiyi bu şekilde Event Store ye gönderiyoruz;
            arg1.Acknowledge(arg2.Event.EventId);
            //evet bana geldi ,ben bu eventi işledim , artık bu eventi bana tekrar gönderme demiş olduk.
        }
    }
}
