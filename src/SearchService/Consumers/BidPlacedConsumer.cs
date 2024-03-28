using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming Bid Placed");
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if ((auction.CurrentHighBid < context.Message.Amount)
            && context.Message.BidStatus == BidStatus.Accepted.ToString())
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }

        Console.WriteLine("<-- Consuming Bid Placed");
    }
}
