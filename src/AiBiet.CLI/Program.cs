using AiBiet.CLI.Bootstrap;
using AiBiet.CLI.Infrastructure;

var appConfig = await ConfigBootstrapper.InitializeAsync().ConfigureAwait(false);

var services = ServiceRegistration.Configure(appConfig);

var app = CliBootstrapper.Build(services);

args = ArgumentProcessor.Normalize(args);

return await app.RunAsync(args).ConfigureAwait(false);