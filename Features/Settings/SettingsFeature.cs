using Microsoft.Extensions.DependencyInjection;
using NtfyDesktop.Features.Notifications;

namespace NtfyDesktop.Features.Settings;

public static class SettingsFeature
{
    extension(IServiceCollection services)
    {
        public void AddSettings()
        {
            services.AddSingleton<AppSettings>(_ => AppSettings.Load());
            
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
        }
        
    }
}