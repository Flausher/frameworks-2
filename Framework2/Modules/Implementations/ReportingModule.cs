using Framework2.Modules.Contracts;
using Framework2.Modules.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Framework2.Modules.Implementations;

public sealed class ReportingModule : IAppModule
{
    public string Name => "Reporting";

    public IReadOnlyCollection<string> RequiredModules => ["Validation"];

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IReportExporter, ReportExporter>();
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        var validation = serviceProvider.GetRequiredService<IValidationService>();
        var exporter = serviceProvider.GetRequiredService<IReportExporter>();

        var rawScores = new[] { 90, 120, 67, -5, 83 };
        var validScores = validation.ValidateScores(rawScores);
        exporter.Export(validScores);
    }
}
