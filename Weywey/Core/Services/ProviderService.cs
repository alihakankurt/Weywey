using System;
using Microsoft.Extensions.DependencyInjection;

namespace Weywey.Core.Services
{
    public static class ProviderService
    {
        public static IServiceProvider Provider { get; private set; }

        public static void SetProvider(IServiceCollection collection)
            => Provider = collection.BuildServiceProvider();
        
        public static T GetService<T>()
            => Provider.GetRequiredService<T>();
    }
}