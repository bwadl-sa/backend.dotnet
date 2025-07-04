using Bwadl.Application.Common.Interfaces;
using Bwadl.Infrastructure.Configuration;
using Bwadl.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bwadl.Infrastructure.ExternalServices;

public class EnhancedEmailService : IEmailService
{
    private readonly IConfigurationService _configurationService;
    private readonly ISecretManager _secretManager;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnhancedEmailService> _logger;

    public EnhancedEmailService(
        IConfigurationService configurationService,
        ISecretManager secretManager,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<EnhancedEmailService> logger)
    {
        _configurationService = configurationService;
        _secretManager = secretManager;
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // Check if email notifications are enabled
            var features = await _configurationService.GetOptionsAsync<FeatureOptions>();
            if (!features.EnableEmailNotifications)
            {
                _logger.LogInformation("Email notifications are disabled. Skipping email to {To}", to);
                return;
            }

            // Get email service configuration
            var emailConfig = await GetEmailServiceConfigurationAsync();
            
            // Get the API key from secret manager
            var apiKey = await _secretManager.GetSecretAsync("ExternalServices:EmailService:ApiKey");

            _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);

            // Configure HTTP client
            _httpClient.BaseAddress = new Uri(emailConfig.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(emailConfig.TimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var emailRequest = new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[] { new { email = to } },
                        subject = subject
                    }
                },
                content = new[]
                {
                    new
                    {
                        type = "text/html",
                        value = body
                    }
                },
                from = new { email = "noreply@bwadl.com", name = "Bwadl System" }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(emailRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/v3/mail/send", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {To}. Status: {StatusCode}, Error: {Error}",
                    to, response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to send email: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            throw;
        }
    }

    private async Task<ExternalServiceOptions> GetEmailServiceConfigurationAsync()
    {
        var emailConfig = new ExternalServiceOptions();
        _configuration.GetSection("ExternalServices:EmailService").Bind(emailConfig);
        
        return await Task.FromResult(emailConfig);
    }
}

public class ExternalServiceOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
}
