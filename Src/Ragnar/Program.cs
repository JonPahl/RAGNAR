Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var culture = General.Culture;

// 2. Set globally for all future threads in the application domain
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// 3. Set for the current startup thread immediately
CultureInfo.CurrentCulture = culture;
CultureInfo.CurrentUICulture = culture;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

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
    await Log.CloseAndFlushAsync()
        .ConfigureAwait(false);
}
