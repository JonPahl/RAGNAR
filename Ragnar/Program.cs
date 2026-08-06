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
catch(Exception Ex)
{
    Log.Fatal(Ex, "Application terminated unexpectedly");
    AnsiConsole.WriteException(Ex);
}
finally
{
    await Log.CloseAndFlushAsync();
}
