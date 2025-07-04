namespace Bwadl.Shared.Configuration;

public class FeatureOptions
{
    public const string SectionName = "Features";
    
    public bool EnableCaching { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnableAnalytics { get; set; } = false;
    public bool EnableEventDrivenArchitecture { get; set; } = true;
    public bool EnableRateLimiting { get; set; } = true;
}
