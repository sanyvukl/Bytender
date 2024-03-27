using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public static class DbInitializer
{
    public static async void Initialize(WebApplication app)
    {
        try
        {
            string connectionString = app.Configuration.GetConnectionString("MONGO_DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("MONGO_DB_CONNECTION_STRING is unset");
                return;
            }

            await DB.InitAsync("SearchDB", MongoClientSettings.FromConnectionString(connectionString));

            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();

            var count = await DB.CountAsync<Item>();

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionHttpClient>();

            var items = await httpClient.GetItemsForSearchDBAsync();

            if (items.Count > 0)
            {
                await DB.SaveAsync(items);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}
