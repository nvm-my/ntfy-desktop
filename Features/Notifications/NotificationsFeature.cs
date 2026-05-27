using Microsoft.Extensions.DependencyInjection;

namespace NtfyDesktop.Features.Notifications;

public static class NotificationsFeature
{
    extension(IServiceCollection services)
    {
        public void AddNotifications()
        {
            services.AddSingleton<NotificationGate>();

            services.AddSingleton<ToastNotifier>(_ =>
            {
                var notifier = new ToastNotifier();
                notifier.Register();
                return notifier;
            });

        }
        
    }
}