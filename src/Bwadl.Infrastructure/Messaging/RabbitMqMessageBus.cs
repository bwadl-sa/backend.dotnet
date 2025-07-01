using Bwadl.Application.Common.Interfaces;
using Serilog;

namespace Bwadl.Infrastructure.Messaging;

public class RabbitMqMessageBus : IMessageBus
{
    private readonly ILogger _logger = Log.ForContext<RabbitMqMessageBus>();

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        _logger.Information("Publishing message of type {MessageType}", typeof(T).Name);
        
        // Simulate message publishing
        await Task.Delay(50, cancellationToken);
        
        _logger.Information("Message published successfully");
    }

    public async Task SubscribeAsync<T>(Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        _logger.Information("Subscribing to messages of type {MessageType}", typeof(T).Name);
        
        // Simulate subscription setup
        await Task.Delay(10, cancellationToken);
        
        _logger.Information("Subscription setup completed");
    }
}
