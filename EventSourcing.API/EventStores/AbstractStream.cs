using EventSourcing.Shared.Events;
using EventStore.ClientAPI;
using System.Text;
using System.Text.Json;

namespace EventSourcing.API.EventStores
{
    public abstract class AbstractStream
    {
        protected readonly LinkedList<IEvent> Events = new LinkedList<IEvent>();
        private string _streamName { get;  }

        private readonly IEventStoreConnection _eventStoreConnection;

        protected AbstractStream(string streamName, IEventStoreConnection eventStoreConnection)
        {
            _streamName = streamName;
            _eventStoreConnection = eventStoreConnection;
        }
        //event store a event kaydedebilmemiz için mutlaka EventData oluşturmalıyız o dayine EventStore.Client paketinten geliyor.
        public async Task SaveAsync()
        {
            //buraya gelen eventler bu şekilde bir event dataya dönüştürülüyor.
            var newEvents = Events.ToList().Select(x =>
            new EventData(
                Guid.NewGuid(),
                x.GetType().Name,
                true,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(x,inputType:x.GetType())),
                Encoding.UTF8.GetBytes(x.GetType().FullName)
                )).ToList();

            await _eventStoreConnection.AppendToStreamAsync(_streamName, ExpectedVersion.Any,newEvents);
            Events.Clear();
        }
    }
}
