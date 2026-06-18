using Microsoft.Extensions.DependencyInjection;

namespace Framework2.Modules.Contracts;

public interface IAppModule
{
    string Name { get; }

    IReadOnlyCollection<string> RequiredModules { get; }

    void RegisterServices(IServiceCollection services);

    void Initialize(IServiceProvider serviceProvider);
}
