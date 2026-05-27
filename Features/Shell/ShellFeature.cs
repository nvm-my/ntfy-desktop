using Microsoft.Extensions.DependencyInjection;

namespace NtfyDesktop.Features.Shell;

public static class ShellFeature
{
    extension(IServiceCollection services)
    {
        public void AddShell()
        {
            // MainWindow and its top-level VMs are singletons so the user can hide/show
            // without losing feed state, search/filter values, or scroll position.
            
            // Pages are transient — each navigation gets a fresh instance, but the
            // VM injected into them is a singleton so state persists across navigations.

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
        }
        
    }
}