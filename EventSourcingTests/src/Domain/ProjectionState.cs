namespace EventSourcingTests.Events;

public class ProjectionState
{
    public int EventCount { get; set; }
    public DateTime LastEventTimestamp { get; set; }
    public int LastEventVersion { get; set; }
}