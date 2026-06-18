namespace Framework2.Core;

public sealed class ModuleRuntime : IDisposable
{
    public ModuleRuntime(IServiceProvider services, IReadOnlyList<string> orderedModuleNames)
    {
        Services = services;
        OrderedModuleNames = orderedModuleNames;
    }

    public IServiceProvider Services { get; }

    public IReadOnlyList<string> OrderedModuleNames { get; }

    public void Dispose()
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
