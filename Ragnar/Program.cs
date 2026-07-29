Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

try
{
    var builder = RagPipelineHostBuilder
        .CreateDefaultBuilder(args);

    using var host = builder.Build();

    await host.RunAsync();

    AnsiConsole.Console.WriteLine("Processes completed.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    AnsiConsole.WriteException(ex);
}
finally
{
    await Log.CloseAndFlushAsync();
}
