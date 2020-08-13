using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core;

namespace Laso.AdminPortal.Infrastructure.Secrets
{
    public class InMemoryApplicationSecrets : IApplicationSecrets
    {
        public readonly ConcurrentDictionary<string, KeyVaultSecret> Secrets = new ConcurrentDictionary<string, KeyVaultSecret>(); 
        
        public Task<KeyVaultSecret> GetSecret(string name, CancellationToken cancellationToken)
        {
            return Task.FromResult(!Secrets.ContainsKey(name) ? default : Secrets[name]);
        }
    }
}
