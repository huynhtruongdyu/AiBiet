// Licensed under the MIT License.
// Copyright (c) 2025 HuynhTruongDyu

using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;

namespace AiBiet.Infrastructure;

public class AiProviderResolver(IAiProviderFactory factory)
{
    public IAiProvider Resolve(AiBietConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var providerName = config.DefaultProvider;
        if (string.IsNullOrEmpty(providerName))
        {
            return factory.GetDefaultProvider();
        }

        return factory.GetProvider(providerName);
    }
}