using System.Text;

using Serilog;

Console.OutputEncoding = Encoding.UTF8;

try
{
    IHostBuilder builder = RagPipelineHostBuilder
        .CreateDefaultBuilder(args);

    using var host = builder.Build();

    await host.RunAsync();

    AnsiConsole.Console.WriteLine("Processes completed.");
}
catch (Exception ex)
{
    Log.Fatal("Application terminated unexpectedly");
    AnsiConsole.WriteException(ex);
}
finally
{
    await Log.CloseAndFlushAsync();
}
