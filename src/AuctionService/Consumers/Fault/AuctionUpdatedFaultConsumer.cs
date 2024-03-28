using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionUpdatedFaultConsumer : IConsumer<Fault<AuctionUpdated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionUpdated>> context)
    {
        Console.WriteLine($"--> Consuming Faulty Creation");
        var exception = context.Message.Exceptions.First();

        if (exception.ExceptionType == "One we expect")
        {
            //Changing a property
            await context.Publish(context.Message.Message);
            Console.WriteLine($"<-- Successfully Republished AuctionUpdated Event id: {context.Message.Message.Id}");
        }
        else
        {
            Console.WriteLine($"<-- Unhandled Exception: {exception.Message}");
        }
    }
}