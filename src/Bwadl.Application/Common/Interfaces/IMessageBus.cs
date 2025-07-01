namespace Bwadl.Application.Common.Interfaces;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    Task SubscribeAsync<T>(Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;
}
