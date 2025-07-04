namespace Bwadl.Shared.Configuration;

public class MessageBusOptions
{
    public const string SectionName = "MessageBus";
    
    public string Provider { get; set; } = "RabbitMq";
    public RabbitMqOptions RabbitMq { get; set; } = new();
}

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "events";
    public string QueueName { get; set; } = "queue";
}
