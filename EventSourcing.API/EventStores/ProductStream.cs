using EventSourcing.API.Dtos;
using EventSourcing.Shared.Events;
using EventStore.ClientAPI;

namespace EventSourcing.API.EventStores
{
    public class ProductStream : AbstractStream
    {
        //buradakinin anlamı sadece get yapılıyor set bloğu olmayan property demektir.
        public static string StreamName => "ProductStream";
        public ProductStream(IEventStoreConnection eventStoreConnection) : base(StreamName, eventStoreConnection)
        {
        }

        public void Created(CreateProductDto createProductDto)
        {
            //linked List in özellikleridir buradakiler
            Events.AddLast(new ProductCreatedEvent 
            { 
                Id=Guid.NewGuid(),
                Name=createProductDto.Name,
                Price=createProductDto.Price,
                Stock=createProductDto.Stock,
                UserId=createProductDto.UserId
            });
        }
        public void NameChanged(ChangeProductNameDto changeProductNameDto)
        {
            Events.AddLast(new ProductNameChangedEvent
            {
                Id = Guid.NewGuid(),
                ChangedName = changeProductNameDto.Name,
            });
        }
        public void PriceChanged(ChangeProductPriceDto changeProductPriceDto)
        {
            Events.AddLast(new ProductPriceChangedEvent
            {
                Id = Guid.NewGuid(),
                ChangedPrice = changeProductPriceDto.Price,
            });
        }
        public void Deleted(Guid id)
        {
            Events.AddLast(new ProductDeletedEvent
            {
                Id = id, 
            });
        }
    }
}
