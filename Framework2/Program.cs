using Framework2.Configuration;
using Framework2.Core;

try
{
	var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
	var settings = ModuleSettingsLoader.Load(settingsPath);
	var modulesDirectory = Path.Combine(AppContext.BaseDirectory, settings.Directory);

	var discoveredModules = ModuleDiscovery.Discover(modulesDirectory);
	var orderedModules = ModuleBootstrapper.ResolveOrder(discoveredModules, settings.EnabledModules);

	using var runtime = ModuleBootstrapper.StartModules(orderedModules);
	Console.WriteLine($"Startup completed. Module order: {string.Join(", ", runtime.OrderedModuleNames)}");
}
catch (ModuleConfigurationException ex)
{
	Console.Error.WriteLine($"Module startup failed: {ex.Message}");
	Environment.ExitCode = 1;
}