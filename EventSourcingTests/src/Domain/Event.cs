using System.Text.Json.Serialization;

namespace EventSourcingTests.Events;
public abstract record Event
{
    public Guid AggregateId { get; set; }
    public DateTimeOffset OccurredOn { get; protected set; }
    public string Name => GetType().Name;
    public abstract string StreamName();

    protected Event(Guid aggregateId, DateTimeOffset occurredOn)
    {
        AggregateId = aggregateId;
        OccurredOn = occurredOn;
    }
}

public record UploadDocument
(
[property: JsonPropertyName("UserId")] string UserId, 
[property: JsonPropertyName("AdminId")] int AdminId, 
[property: JsonPropertyName("TenderId")] Guid TenderId, 
[property: JsonPropertyName("DocumentId")] Guid DocumentId, 
[property: JsonPropertyName("VersionNr")] int VersionNr,
[property: JsonPropertyName("OccurredOn")] DateTimeOffset OccurredOn
    ) : Event(DocumentId, OccurredOn)
{
    public override string StreamName() => $"Tender-{TenderId}-Document-{DocumentId}";
}

// edit document done by an admin user. Aggregate is documentID since event stream should be connected by each document
    public record EditDocument
    (
        [property: JsonPropertyName("UserId")] string UserId, 
        [property: JsonPropertyName("AdminId")] int AdminId, 
        [property: JsonPropertyName("TenderId")] Guid TenderId, 
        [property: JsonPropertyName("DocumentId")] Guid DocumentId, 
        [property: JsonPropertyName("VersionNr")] int VersionNr, 
        [property: JsonPropertyName("Reason")] string Reason, 
        [property: JsonPropertyName("UserLocation")] string UserLocation, 
        [property: JsonPropertyName("OccurredOn")] DateTimeOffset OccurredOn
        ) : Event(DocumentId, OccurredOn) 
    {
        public override string StreamName() => $"Tender-{TenderId}-Document-{DocumentId}";
    }

public record DeleteDocument
(
    [property: JsonPropertyName("UserId")] string UserId, 
    [property: JsonPropertyName("AdminId")] int AdminId, 
    [property: JsonPropertyName("TenderId")] Guid TenderId, 
    [property: JsonPropertyName("DocumentId")] Guid DocumentId, 
    [property: JsonPropertyName("VersionNr")] int VersionNr, 
    [property: JsonPropertyName("OccurredOn")] DateTimeOffset OccurredOn
) : Event(DocumentId, OccurredOn)
{
    public override string StreamName() => $"Tender-{TenderId}-Document-{DocumentId}";
}



    //BidTender is a record that represents a bid on a tender. Aggregate is TenderId since event stream should be connected by each tender
    public record BidTender(string UserId, Guid TenderId, int Amount, int VersionNr, DateTimeOffset OccurredOn) : Event(TenderId, OccurredOn)
    {
        public BidTender(string UserId, Guid TenderId, int Amount, int VersionNr) : this(UserId, TenderId, Amount, VersionNr,
            DateTimeOffset.UtcNow)
        {
        }

        public override string StreamName() => $"Tender-{TenderId}";
    }

    //ChangeBidTender is a record that represents a change in a bid on a tender. Aggregate is TenderId since event stream should be connected by each tender
    public record ChangeBidTender
        (string UserId, Guid TenderId, int Amount, int VersionNr, DateTimeOffset OccurredOn) : Event(TenderId, OccurredOn)
    {
        public ChangeBidTender(string UserId, Guid TenderId, int Amount, int VersionNr) : this(UserId, TenderId, Amount, VersionNr,
            DateTimeOffset.UtcNow)
        {
        }

        public override string StreamName() => $"Tender-{TenderId}";
    }

    //RemoveBidTender is a record that represents a removal of a bid on a tender. Aggregate is TenderId since event stream should be connected by each tender
    public record RemoveBidTender(string UserId, Guid TenderId, int VersionNr, DateTimeOffset OccurredOn) : Event(TenderId, OccurredOn)
    {
        public RemoveBidTender(string UserId, Guid TenderId, int VersionNr) : this(UserId, TenderId, VersionNr, DateTimeOffset.UtcNow)
        {
        }

        public override string StreamName() => $"Tender-{TenderId}";
    }

