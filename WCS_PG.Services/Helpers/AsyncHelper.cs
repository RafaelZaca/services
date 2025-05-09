// WCS_PG.Services/Helpers/AsyncHelper.cs

using Microsoft.Extensions.DependencyInjection;
using System;

namespace WCS_PG.Services.Helpers
{
    public static class ServiceProviderHelper
    {
        public static IServiceProvider ServiceProvider { get; set; }
    }
    public class AsyncHelper
    {
        public AsyncScope CreateScope()
        {
            return new AsyncScope(ServiceProviderHelper.ServiceProvider.CreateScope());
        }

        public class AsyncScope : IDisposable
        {
            private readonly IServiceScope _serviceScope;

            public AsyncScope(IServiceScope serviceScope)
            {
                _serviceScope = serviceScope;
                ServiceProvider = serviceScope.ServiceProvider;
            }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose()
            {
                _serviceScope?.Dispose();
            }
        }
    }
}