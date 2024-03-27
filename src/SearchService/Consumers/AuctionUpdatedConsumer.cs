using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        this._mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"--> Consuming AuctionUpdated Event, id: {context.Message.Id}");
        var item = this._mapper.Map<Item>(context.Message);

        Console.WriteLine("Succesfuly Mapped AuctionUpdated Event to Item");

        var result = await DB.Update<Item>()
            .MatchID(context.Message.Id)
            .ModifyWith(item)
            .ExecuteAsync();

        Console.WriteLine("Succesfuly Updated Item in Mongo DB");

        if (!result.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionUpdated), "Problem updating in MongoDB");
        }

        Console.WriteLine($"<-- Successfuly Updated Auction in MongoDB, id: {context.Message.Id}");
    }
}
