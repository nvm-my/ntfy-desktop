using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NtfyDesktop.Features.Settings;

namespace NtfyDesktop.Features.Rules;

public static class RulesFeature
{
    extension(IServiceCollection services)
    {
        public void AddRules()
        {
            services.AddSingleton<IIncidentStore>(sp => new IncidentStore(
                Path.Combine(App.DataPath, "rules.db"),
                sp.GetRequiredService<AppSettings>().GetOrCreateHistoryKey()));

            services.AddSingleton<PackStore>(_ => new PackStore(
                Path.Combine(App.DataPath, "rules")));

            services.AddSingleton<RuleEngine>();

            services.AddSingleton<ExpectationStore>(sp => new ExpectationStore(
                Path.Combine(App.DataPath, "rules.db"),
                sp.GetRequiredService<AppSettings>().GetOrCreateHistoryKey()));

            services.AddHostedService<ExpectationMonitor>();
        }
    }
}
