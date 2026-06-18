using Framework2.Modules.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Framework2.Core;

public static class ModuleBootstrapper
{
    public static IReadOnlyList<IAppModule> ResolveOrder(
        IReadOnlyCollection<IAppModule> discoveredModules,
        IReadOnlyCollection<string> enabledModules)
    {
        var moduleMap = discoveredModules.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
        var enabledSet = new HashSet<string>(enabledModules, StringComparer.OrdinalIgnoreCase);

        foreach (var enabledModule in enabledSet)
        {
            if (!moduleMap.ContainsKey(enabledModule))
            {
                throw new ModuleConfigurationException($"Module '{enabledModule}' is listed in configuration but was not found.");
            }
        }

        var order = new List<IAppModule>();
        var states = new Dictionary<string, VisitState>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<string>();

        foreach (var moduleName in enabledSet)
        {
            Visit(moduleName, moduleMap, enabledSet, states, stack, order);
        }

        return order;
    }

    public static ModuleRuntime StartModules(IReadOnlyList<IAppModule> orderedModules)
    {
        var services = new ServiceCollection();

        foreach (var module in orderedModules)
        {
            module.RegisterServices(services);
        }

        var provider = services.BuildServiceProvider();

        foreach (var module in orderedModules)
        {
            module.Initialize(provider);
        }

        return new ModuleRuntime(provider, orderedModules.Select(m => m.Name).ToList());
    }

    private static void Visit(
        string moduleName,
        IReadOnlyDictionary<string, IAppModule> moduleMap,
        IReadOnlySet<string> enabledModules,
        IDictionary<string, VisitState> states,
        Stack<string> stack,
        ICollection<IAppModule> order)
    {
        if (states.TryGetValue(moduleName, out var state))
        {
            if (state == VisitState.Visited)
            {
                return;
            }

            if (state == VisitState.Visiting)
            {
                var cycle = stack.Reverse().SkipWhile(name => !name.Equals(moduleName, StringComparison.OrdinalIgnoreCase)).ToList();
                cycle.Add(moduleName);
                throw new ModuleConfigurationException($"Cyclic dependency detected: {string.Join(" -> ", cycle)}.");
            }
        }

        states[moduleName] = VisitState.Visiting;
        stack.Push(moduleName);

        var module = moduleMap[moduleName];
        foreach (var requiredModule in module.RequiredModules)
        {
            if (!enabledModules.Contains(requiredModule))
            {
                throw new ModuleConfigurationException(
                    $"Module '{moduleName}' requires '{requiredModule}', but it is not enabled or not found.");
            }

            Visit(requiredModule, moduleMap, enabledModules, states, stack, order);
        }

        stack.Pop();
        states[moduleName] = VisitState.Visited;

        if (!order.Contains(module))
        {
            order.Add(module);
        }
    }

    private enum VisitState
    {
        NotVisited = 0,
        Visiting = 1,
        Visited = 2
    }
}
