namespace Quickstart.Common;

internal static class Cleanup
{
    public static void Try(string asset, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"Deleted {asset}");
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Could not delete {asset}: {exception.Message}");
        }
    }
}
