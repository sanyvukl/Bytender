using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionDeletedFaultConsumer : IConsumer<Fault<AuctionDeleted>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionDeleted>> context)
    {
        Console.WriteLine($"--> Consuming Faulty Creation");
        var exception = context.Message.Exceptions.First();

        if (exception.ExceptionType == "One we expect")
        {
            //Changing a property
            await context.Publish(context.Message.Message);
            Console.WriteLine($"<-- Successfully Republished AuctionDeleted Event, id: {context.Message.Message.Id}");
        }
        else
        {
            Console.WriteLine($"<-- Unhandled Exception: {exception.Message}");
        }
    }

}
