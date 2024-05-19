using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Infrastructure;
using Core.Services;
using EventSourcingTests.Events;
using EventStore.Client;


namespace Core;

public class EventConverter : JsonConverter<Event>
{
    public override Event Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;
            string type = root.GetProperty("Name").GetString();
            
            switch (type)
            {
                case "UploadDocument":
                    return DeserializeEvent<UploadDocument>(root, options);
                case "EditDocument":
                    var userLocation = root.TryGetProperty("UserLocation", out var location) ? location.GetString() : "Unknown";
                    return DeserializeEvent<EditDocument>(root, options);
                case "DeleteDocument":
                    return DeserializeEvent<DeleteDocument>(root, options);
                default:
                    throw new JsonException($"Unknown type {type}");
            }
        }
    }

    private TEvent DeserializeEvent<TEvent>(JsonElement element, JsonSerializerOptions options) where TEvent : Event
    {
    
        var jsonText = element.GetRawText();
       // Console.WriteLine(jsonText);
        try {
            return JsonSerializer.Deserialize<TEvent>(jsonText, options);
        } catch (JsonException ex) {
            Console.WriteLine($"Error during deserialization: {ex.Message}");
            throw;
        }
    }

    public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var eventStoreService = host.Services.GetRequiredService<IEventStore>();
        var reconstructor = host.Services.GetRequiredService<TenderStateReconstructor>();
        var documentCommandService = host.Services.GetRequiredService<DocumentCommandService>();
        var documentQueryService = host.Services.GetRequiredService<DocumentQueryService>();
        var httpClient = new HttpClient();
        var eventStoreClient = new EventStoreProjectionService(httpClient);
        // Process predefined events
        // await ProcessPredefinedBidEvents(eventStoreService, reconstructor);
        await ProcessPredefinedDocumentEvents(eventStoreService, documentCommandService, documentQueryService);
        try
        {
            var projectionState = await eventStoreClient.GetProjectionStateAsync("TestEventCount");
            Console.WriteLine($"Event Count: {projectionState.EventCount}");
            Console.WriteLine($"Last Event Timestamp: {projectionState.LastEventTimestamp}");
            Console.WriteLine($"Last Event Version: {projectionState.LastEventVersion}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }


        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IEventStore, EventStoreService>();
                services.AddSingleton<IFileSystem, FileSystem>();
                services.AddTransient<DocumentCommandService>();
                services.AddTransient<DocumentQueryService>();
                ConfigureServices(services);
            });

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventStore, EventStoreService>();
        services.AddTransient<TenderStateReconstructor>();
    }

    private static async Task ProcessPredefinedDocumentEvents(IEventStore eventStoreService,
        DocumentCommandService documentCommandService, DocumentQueryService documentQueryService)
    {
        Guid TenderAggregateId = Guid.NewGuid();
        Guid DocumentAggregateId1 = Guid.NewGuid();
        Guid DocumentAggregateId2 = Guid.NewGuid();
        Console.WriteLine($"Generated AggregateIDs: {DocumentAggregateId1}, {DocumentAggregateId2}");
        //await documentCommandService.DeleteDocument("Admin1", 1, TenderAggregateId, DocumentAggregateId1, "NewDocument3.txt");


        //Url for a projection state
        var url = $"http://localhost:2113/projection/{"TestEventCount"}/state"; // Adjust the URL to match the EventStoreDB setup




        //Events being called to add to event store. Delay is added for visibility live in the event store
        await documentCommandService.UploadDocument("Admin1", 1, TenderAggregateId, DocumentAggregateId2,
            "NewDocument6.txt", "This file has now been created");
        await Task.Delay(5000); // 5 seconds delay

        for (int i = 0; i < 10; i++)
        {
            await documentCommandService.EditDocument("Admin2", 2, TenderAggregateId, DocumentAggregateId2,
                "NewDocument6.txt",
                "File contains wrong information", "Copenhagen, Denmark", "This is the updated info in the file");

            await Task.Delay(5000);
        }
        
        await documentCommandService.DeleteDocument("Admin1", 1, TenderAggregateId, DocumentAggregateId2,
            "NewDocument6.txt");
        await Task.Delay(5000); 
        // await documentCommandService.UploadDocument("Admin1", 1, TenderAggregateId, DocumentAggregateId1, "NewDocument3.txt"
        // ,"This is the contents of my file");


        var options = new JsonSerializerOptions
        {
            Converters = { new EventConverter() }
        };

        var documentEventStream =
            documentQueryService.GetDocumentEvents(DocumentAggregateId2, TenderAggregateId, options);

        var uniqueStreamNames = documentEventStream.Result.Select(e => e.StreamName()).Distinct();

        // Fetch events for each unique stream name
        List<Event> events = new List<Event>();
        foreach (var streamName in uniqueStreamNames)
        {
            var temp = await eventStoreService.GetEventsAsync(streamName, options);
            events.AddRange(temp);
        }


        foreach (var eventt in events)
        {
            Console.WriteLine("FoundEvent:");
            Console.WriteLine(eventt);
        }
    }
}
/* Not used for current implementation regarding documents.
private static async Task ProcessPredefinedBidEvents(IEventStore eventStoreService, TenderStateReconstructor reconstructor)
{
    Guid tenderAggregateId1 = Guid.NewGuid();
    Guid tenderAggregateId2 = Guid.NewGuid();
    Console.WriteLine($"Generated AggregateIDs: {tenderAggregateId1}, {tenderAggregateId2}");

    var eventsToAdd = new List<Event> {
        new BidTender("user1", tenderAggregateId1, 100, 1),
        new BidTender("user2", tenderAggregateId1, 200, 1),
        new BidTender("user3", tenderAggregateId1, 300, 1),
        new BidTender("user5", tenderAggregateId1, 300, 1),
        new BidTender("user5", tenderAggregateId2, 400, 1),
        new ChangeBidTender("user5", tenderAggregateId2, 900, 1),
        new ChangeBidTender("user1", tenderAggregateId1, 250, 1),
        new BidTender("user3", tenderAggregateId2, 400, 1),
        new RemoveBidTender("user3", tenderAggregateId1, 1),
    };

    foreach (var eventToAdd in eventsToAdd)
    {
        await eventStoreService.AppendEventAsync(eventToAdd);
    }

    
    // Example to reconstruct state to validate setup - not updated for current code
   // var tenderTable1 = await reconstructor.ReconstructStateAsync(tenderAggregateId1);
   // var tenderTable2 = await reconstructor.ReconstructStateAsync(tenderAggregateId2);

   // tenderTable1.PrintTableMockup();
    //tenderTable2.PrintTableMockup();
}
}
*/


