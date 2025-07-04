namespace Bwadl.Shared.Configuration;

public class ApplicationOptions
{
    public const string SectionName = "Application";
    
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}
