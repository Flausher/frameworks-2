using System.Text.Json;
using Framework2.Core;

namespace Framework2.Configuration;

public static class ModuleSettingsLoader
{
    public static ModuleSettings Load(string settingsPath)
    {
        if (!File.Exists(settingsPath))
        {
            throw new ModuleConfigurationException($"Settings file '{settingsPath}' was not found.");
        }

        var json = File.ReadAllText(settingsPath);
        var root = JsonSerializer.Deserialize<RootSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (root?.Modules is null)
        {
            throw new ModuleConfigurationException("Missing 'Modules' section in settings file.");
        }

        return root.Modules;
    }

    private sealed class RootSettings
    {
        public ModuleSettings? Modules { get; set; }
    }
}
