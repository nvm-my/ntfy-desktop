using Microsoft.Extensions.DependencyInjection;

namespace NtfyDesktop.Features.History;

public static class HistoryFeature
{
    extension(IServiceCollection services)
    {
        public void AddHistory()
        {
            services.AddSingleton<HistoryRepository>();
            
            // retention sweeps run in HistoryRetentionService at startup and also 
            // at the specified interval in the service
            services.AddHostedService<HistoryRetentionService>();
        }
        
    }
}