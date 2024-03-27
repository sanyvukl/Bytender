namespace SearchService.Helpers;

public static class ExceptionHandler
{
    public static void HandleException(Exception e, string place)
    {
        Console.WriteLine($"An error occurred in {place}. Message {e.Message}");
    }
}
