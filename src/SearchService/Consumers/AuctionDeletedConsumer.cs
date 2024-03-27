using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine($"--> Consuming AuctionDeleted Event, id: {context.Message.Id}");
        await DB.DeleteAsync<Item>(context.Message.Id);
        Console.WriteLine($"<-- Successfuly Deleted Auction from MongoDB, id: {context.Message.Id}");
    }
}
