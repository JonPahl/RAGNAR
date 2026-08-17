Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

try
{
    var Builder = RagPipelineHostBuilder
        .CreateDefaultBuilder(args);

    using var Host = Builder.Build();

    await Host.RunAsync();

    AnsiConsole.Console.WriteLine("Processes completed.");
}
catch(Exception ex)
{
    Log.Fatal("Application terminated unexpectedly");
    AnsiConsole.WriteException(ex);
}
finally
{
    await Log.CloseAndFlushAsync();
}
