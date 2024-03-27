using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        this._mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"--> Consuming AuctionCreated Event id: {context.Message.Id}");
        var item = this._mapper.Map<Item>(context.Message);

        await item.SaveAsync();
        Console.WriteLine($"<-- Successfuly Saved AuctionCreated in MongoDB id: {context.Message.Id}");
    }
}
