using AiBiet.CLI.Bootstrap;
using AiBiet.CLI.Infrastructure;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var appConfig = await ConfigBootstrapper.InitializeAsync().ConfigureAwait(false);

var services = ServiceRegistration.Configure(appConfig);

var app = CliBootstrapper.Build(services);

args = ArgumentProcessor.Normalize(args);

return await app.RunAsync(args).ConfigureAwait(false);