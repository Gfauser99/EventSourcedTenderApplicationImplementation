using System.Text.Json;
using Core.Infrastructure;
using EventSourcingTests.Events;

namespace Core.Services;

// Made for visualization of a 'projection' of a particular table's bidding status


public class TenderStateReconstructor
{
    private readonly IEventStore _eventStore;

    public TenderStateReconstructor(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<TenderTableOverview> ReconstructStateAsync(string streamName, JsonSerializerOptions options)
    {
        var events = await _eventStore.GetEventsAsync(streamName, options);
        var tenderTable = new TenderTableOverview();

        foreach (var singleEvent in events)
        {
            switch (singleEvent)
            {
                case BidTender bt:
                    tenderTable.TenderId = bt.TenderId;
                    tenderTable.BidTenders.Add(new UserBidList { UserId = bt.UserId, Amount = bt.Amount });
                    break;
                case ChangeBidTender cbt:
                    var bid = tenderTable.BidTenders.FirstOrDefault(b => b.UserId == cbt.UserId && cbt.TenderId == cbt.TenderId);
                    if (bid != null) bid.Amount = cbt.Amount;
                    break;
                case RemoveBidTender rbt:
                    tenderTable.BidTenders.RemoveAll(b => b.UserId == rbt.UserId);
                    break;
            }
        }

        if (tenderTable.BidTenders.Any())
            tenderTable.HighestBid = tenderTable.BidTenders.Max(b => b.Amount);
        else
            tenderTable.HighestBid = 0;

        return tenderTable;
    }
    
    public class TenderTableOverview
    {
        public int HighestBid { get; set; }
        public Guid TenderId { get; set; }
        public List<UserBidList> BidTenders { get; set; } = new();

        public void PrintTableMockup()
        {
            Console.WriteLine($"TenderID: {TenderId}");
            Console.WriteLine($"HighestBid: {HighestBid}\n");
            Console.WriteLine($"{"UserID",-10} | {"Amount",10}");
            Console.WriteLine(new string('-', 23));
            foreach (var bid in BidTenders)
            {
                Console.WriteLine($"{bid.UserId,-10} | {bid.Amount,10}");
            }

            Console.WriteLine("\n");
        }
    }
    
    public class UserBidList
    {
        public int Amount { get; set; }
        public string UserId { get; set; }
    }
}
