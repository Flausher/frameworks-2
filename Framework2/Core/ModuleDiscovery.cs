using System.Reflection;
using System.Runtime.Loader;
using Framework2.Modules.Contracts;

namespace Framework2.Core;

public static class ModuleDiscovery
{
    public static IReadOnlyList<IAppModule> Discover(string modulesDirectory)
    {
        var assemblies = new List<Assembly> { Assembly.GetExecutingAssembly() };

        if (Directory.Exists(modulesDirectory))
        {
            foreach (var dllPath in Directory.GetFiles(modulesDirectory, "*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    assemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dllPath)));
                }
                catch
                {
                    // Ignore non-.NET assemblies and keep scanning others.
                }
            }
        }

        var modules = new Dictionary<string, IAppModule>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in assemblies.Distinct())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || !typeof(IAppModule).IsAssignableFrom(type))
                {
                    continue;
                }

                if (type.GetConstructor(Type.EmptyTypes) is null)
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is not IAppModule module)
                {
                    continue;
                }

                if (modules.ContainsKey(module.Name))
                {
                    throw new ModuleConfigurationException($"Duplicate module name detected: '{module.Name}'.");
                }

                modules[module.Name] = module;
            }
        }

        return modules.Values.ToList();
    }
}
