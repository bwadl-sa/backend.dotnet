namespace Bwadl.Shared.Configuration;

public class SecurityOptions
{
    public const string SectionName = "Security";
    
    public JwtOptions Jwt { get; set; } = new();
    public ApiKeyOptions ApiKeys { get; set; } = new();
}

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty; // Will be retrieved from secrets
    public int ExpirationMinutes { get; set; } = 60;
}

public class ApiKeyOptions
{
    public bool RequireApiKey { get; set; } = false;
    public List<string> ValidApiKeys { get; set; } = new();
}
