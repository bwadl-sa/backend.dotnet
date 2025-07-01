using Serilog;

namespace Bwadl.Infrastructure.ExternalServices;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly ILogger _logger = Log.ForContext<EmailService>();

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.Information("Sending email to {To} with subject {Subject}", to, subject);
        
        // Simulate email sending
        await Task.Delay(100);
        
        _logger.Information("Email sent successfully to {To}", to);
    }
}
