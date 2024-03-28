using AuctionService.Data;
using AuctionService.Models;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionDbContext _dbContext;

    public BidPlacedConsumer(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming Bid Placed");
        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

        if ((auction.CurrentHightBid == null
            || auction.CurrentHightBid < context.Message.Amount)
            && context.Message.BidStatus == BidStatus.Accepted.ToString())
        {
            auction.CurrentHightBid = context.Message.Amount;
            await _dbContext.SaveChangesAsync();
        }

        Console.WriteLine("<-- Consuming Bid Placed");
    }
}
