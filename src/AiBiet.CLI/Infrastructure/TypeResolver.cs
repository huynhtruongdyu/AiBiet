using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace AiBiet.CLI.Infrastructure;

internal class TypeResolver(ServiceProvider provider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type)
    {
        return type == null ? null : provider.GetService(type);
    }

    public void Dispose()
    {
        provider.Dispose();
    }
}
