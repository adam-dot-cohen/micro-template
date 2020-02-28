using System.Threading;
using System.Threading.Tasks;

namespace Laso.Provisioning.Core
{
    public interface IKeyVaultService
    {
        Task<string> SetSecret(string name, string value, CancellationToken cancellationToken);
        
        Task<string> GetSecret(string name, CancellationToken cancellationToken);
        Task<string> GetSecret(string name, string version, CancellationToken cancellationToken);
    }
}
