namespace Bwadl.Application.Common.Interfaces;

public interface ISecretManager
{
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
    Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);
}
