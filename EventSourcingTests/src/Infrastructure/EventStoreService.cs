using System.Text.Json;
using EventSourcingTests.Events;
using EventStore.Client;

namespace Core.Infrastructure;

public class EventStoreService : IEventStore
{
    private readonly EventStoreClient _client;

    public EventStoreService(IConfiguration configuration)
    {
        var connectionString = configuration["EventStore:ConnectionString"];
        var settings = EventStoreClientSettings.Create(connectionString);
        _client = new EventStoreClient(settings);
    }

    public async Task AppendEventAsync<TEvent>(TEvent @event) where TEvent : Event
    {
        var eventData = new EventData(
            Uuid.NewUuid(),
            @event.GetType().Name,
            JsonSerializer.SerializeToUtf8Bytes(@event),
            JsonSerializer.SerializeToUtf8Bytes(new { @event.OccurredOn })
        );

        await _client.AppendToStreamAsync(
            @event.StreamName(),
            StreamState.Any,
            new[] { eventData }
        );
    }

    public async Task<IEnumerable<Event>> GetEventsAsync(string streamName, JsonSerializerOptions options)
    {
        var events = new List<Event>();
        var result = await _client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start).ToListAsync();

        foreach (var resolvedEvent in result)
        {
            var thisEvent = JsonSerializer.Deserialize<Event>(resolvedEvent.Event.Data.ToArray(), options);
            if (thisEvent != null)
            {
                events.Add(thisEvent);
            }
        }
        return events;
    }
}
