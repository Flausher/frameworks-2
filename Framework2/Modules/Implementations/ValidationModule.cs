using Framework2.Modules.Contracts;
using Framework2.Modules.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Framework2.Modules.Implementations;

public sealed class ValidationModule : IAppModule
{
    public string Name => "Validation";

    public IReadOnlyCollection<string> RequiredModules => ["Logging"];

    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IValidationService, ValidationService>();
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        var auditTrail = serviceProvider.GetRequiredService<IAuditTrail>();
        auditTrail.Write("Validation module initialized.");
    }
}
