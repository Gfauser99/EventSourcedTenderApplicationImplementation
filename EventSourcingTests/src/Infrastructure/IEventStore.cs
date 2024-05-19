using System.Text.Json;
using EventSourcingTests.Events;

namespace Core.Infrastructure;

public interface IEventStore
{
    Task AppendEventAsync<TEvent>(TEvent @event) where TEvent : Event;
    Task<IEnumerable<Event>> GetEventsAsync(string streamName, JsonSerializerOptions options);
}