namespace AiBiet.Core.Interfaces;

public interface IAiProviderFactory
{
    IAiProvider GetDefaultProvider();
    IAiProvider GetProvider(string name);
}
