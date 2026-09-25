Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

try
{
    var builder = RagPipelineHostBuilder
        .CreateDefaultBuilder(args);

    using var host = builder.Build();

    await host.RunAsync().ConfigureAwait(false);

    AnsiConsole.Console.WriteLine("Processes completed.");
}
catch (Exception ex)
{
    Log.Fatal(ex, ex.Message);
    throw;
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
