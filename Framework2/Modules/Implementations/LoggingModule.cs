using Framework2.Modules.Contracts;
using Framework2.Modules.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Framework2.Modules.Implementations;

public sealed class LoggingModule : IAppModule
{
    public string Name => "Logging";

    public IReadOnlyCollection<string> RequiredModules => Array.Empty<string>();

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAuditTrail, AuditTrail>();
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        var auditTrail = serviceProvider.GetRequiredService<IAuditTrail>();
        auditTrail.Write("Logging module initialized.");
    }
}
