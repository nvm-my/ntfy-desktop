using Microsoft.Extensions.DependencyInjection;

namespace NtfyDesktop.Features.Connections;

public static class ConnectionsFeature
{
    extension(IServiceCollection services)
    {
        public void AddConnections()
        {
            services.AddSingleton<ConnectionManager>();
            services.AddHostedService<ConnectionStartupService>();
            
            services.AddSingleton<ConnectionsViewModel>();
            services.AddTransient<ConnectionsPage>();
        }
        
    }
}