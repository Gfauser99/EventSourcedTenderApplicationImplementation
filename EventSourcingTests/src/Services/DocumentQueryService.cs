using System.Text.Json;
using Core.Infrastructure;
using EventSourcingTests.Events;

namespace Core.Services;

public class DocumentQueryService
{
    private readonly IEventStore _eventStore;

    public DocumentQueryService(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<List<Event>> GetDocumentEvents(Guid documentId, Guid tenderId, JsonSerializerOptions options)
    {
        var streamName = $"Tender-{tenderId}-Document-{documentId}";
        var events = await _eventStore.GetEventsAsync(streamName, options);
        return events.ToList();
    }
}