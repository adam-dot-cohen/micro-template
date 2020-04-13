using System.Threading;
using System.Threading.Tasks;

namespace Laso.Provisioning.Core
{
    public interface IApplicationSecrets
    {
        Task<string> SetSecret(string name, string value, CancellationToken cancellationToken);

        Task<bool> SecretExists(string name, CancellationToken cancellationToken);
        Task<bool> SecretExists(string name, string version, CancellationToken cancellationToken);

        Task<string> GetSecret(string name, CancellationToken cancellationToken);
        Task<string> GetSecret(string name, string version, CancellationToken cancellationToken);

        Task DeleteSecret(string name, CancellationToken cancellationToken);
    }
}
