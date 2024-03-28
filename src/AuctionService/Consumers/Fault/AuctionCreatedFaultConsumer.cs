using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine($"--> Consuming Faulty Creation");
        var exception = context.Message.Exceptions.First();

        if (exception.ExceptionType == "One we expect")
        {
            //Changing a property
            context.Message.Message.Color = "white";
            await context.Publish(context.Message.Message);
            Console.WriteLine($"<-- Successfully Republished AuctionCreated Event id: {context.Message.Message.Id}");
        }
        else
        {
            Console.WriteLine($"<-- Unhandled Exception: {exception.Message}");
        }
    }
}
