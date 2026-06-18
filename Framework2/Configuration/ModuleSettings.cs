namespace Framework2.Configuration;

public sealed class ModuleSettings
{
    public string Directory { get; set; } = "modules";

    public List<string> EnabledModules { get; set; } = [];
}
