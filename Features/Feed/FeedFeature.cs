using Microsoft.Extensions.DependencyInjection;
using NtfyDesktop.Features.Connections;

namespace NtfyDesktop.Features.Feed;

public static class FeedFeature
{
    extension(IServiceCollection services)
    {
        public void AddFeeds()
        {
            services.AddSingleton<FeedViewModel>();
            services.AddTransient<FeedPage>();
        }
        
    }
}