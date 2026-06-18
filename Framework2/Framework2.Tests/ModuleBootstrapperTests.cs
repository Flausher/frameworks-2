using Framework2.Core;
using Framework2.Modules.Contracts;
using Framework2.Modules.Implementations;
using Framework2.Modules.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Framework2.Tests;

public sealed class ModuleBootstrapperTests
{
    [Fact]
    public void ResolveOrder_ShouldBuildCorrectOrder_ForLinearDependencies()
    {
        var modules = new IAppModule[]
        {
            new TestModule("A", ["B"]),
            new TestModule("B", ["C"]),
            new TestModule("C", [])
        };

        var ordered = ModuleBootstrapper.ResolveOrder(modules, ["A", "B", "C"]);

        Assert.Equal(["C", "B", "A"], ordered.Select(m => m.Name));
    }

    [Fact]
    public void ResolveOrder_ShouldBuildCorrectOrder_ForBranchingDependencies()
    {
        var modules = new IAppModule[]
        {
            new TestModule("Api", ["Validation", "Storage"]),
            new TestModule("Validation", ["Logging"]),
            new TestModule("Storage", ["Logging"]),
            new TestModule("Logging", [])
        };

        var ordered = ModuleBootstrapper.ResolveOrder(modules, ["Api", "Validation", "Storage", "Logging"]);
        var names = ordered.Select(m => m.Name).ToList();

        Assert.True(names.IndexOf("Logging") < names.IndexOf("Validation"));
        Assert.True(names.IndexOf("Logging") < names.IndexOf("Storage"));
        Assert.True(names.IndexOf("Validation") < names.IndexOf("Api"));
        Assert.True(names.IndexOf("Storage") < names.IndexOf("Api"));
    }

    [Fact]
    public void ResolveOrder_ShouldThrowReadableError_WhenRequiredModuleIsMissing()
    {
        var modules = new IAppModule[]
        {
            new TestModule("A", ["B"]),
            new TestModule("B", [])
        };

        var ex = Assert.Throws<ModuleConfigurationException>(() => ModuleBootstrapper.ResolveOrder(modules, ["A"]));

        Assert.Contains("requires 'B'", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveOrder_ShouldThrowReadableError_WhenCycleExists()
    {
        var modules = new IAppModule[]
        {
            new TestModule("A", ["B"]),
            new TestModule("B", ["C"]),
            new TestModule("C", ["A"])
        };

        var ex = Assert.Throws<ModuleConfigurationException>(() => ModuleBootstrapper.ResolveOrder(modules, ["A", "B", "C"]));

        Assert.Contains("Cyclic dependency detected", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("A", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("B", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("C", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StartModules_ShouldResolveDependencies_FromContainer()
    {
        var modules = new IAppModule[]
        {
            new LoggingModule(),
            new ValidationModule()
        };

        var ordered = ModuleBootstrapper.ResolveOrder(modules, ["Logging", "Validation"]);
        using var runtime = ModuleBootstrapper.StartModules(ordered);

        var auditTrail = runtime.Services.GetRequiredService<IAuditTrail>();
        var validation = runtime.Services.GetRequiredService<IValidationService>();

        var concreteValidation = Assert.IsType<ValidationService>(validation);
        Assert.Same(auditTrail, concreteValidation.AuditTrail);
    }

    private sealed class TestModule : IAppModule
    {
        public TestModule(string name, IReadOnlyCollection<string> requiredModules)
        {
            Name = name;
            RequiredModules = requiredModules;
        }

        public string Name { get; }

        public IReadOnlyCollection<string> RequiredModules { get; }

        public void RegisterServices(IServiceCollection services)
        {
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
        }
    }
}
