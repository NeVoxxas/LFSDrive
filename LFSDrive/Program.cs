using LfsCruise.Core;

namespace LfsCruise;

internal static class Program
{
    private static async Task Main()
    {
        Console.Title = "LFS Cruise";

        WriteStartupLogo();

        var config = new ServerConfig();
        var server = new CruiseServer(config);

        using var shutdown = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            shutdown.Cancel();
        };

        await server.StartAsync(shutdown.Token);
    }

    private static void WriteStartupLogo()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("""
            __    ___________ ____  ____  _____    ________
           / /   / ____/ ___// __ \/ __ \/  _/ |  / / ____/
          / /   / /_   \__ \/ / / / /_/ // / | | / / __/   
         / /___/ __/  ___/ / /_/ / _, _// /  | |/ / /___   
        /_____/_/    /____/_____/_/ |_/___/  |___/_____/
        """);
        Console.ResetColor();
        Console.WriteLine("Starting LFS Cruise server...");
        Console.WriteLine();
    }
}
