using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Core;

namespace Laso.Provisioning.Infrastructure
{
    public class InMemoryApplicationSecrets : IApplicationSecrets
    {
        // NOTE: No version support yet...
        public readonly ConcurrentDictionary<string, string> Secrets = new ConcurrentDictionary<string, string>(); 

        public Task<string> SetSecret(string name, string value, CancellationToken cancellationToken)
        {
            Secrets[name] = value;
            return Task.FromResult("1");
        }

        public Task<bool> SecretExists(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SecretExists(string name, string version, CancellationToken cancellationToken)
        {
            return Task.FromResult(Secrets.TryGetValue(name, out _));
        }

        public Task<string> GetSecret(string name, CancellationToken cancellationToken)
        {
            return GetSecret(name, null, cancellationToken);
        }

        public Task<string> GetSecret(string name, string version, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(version))
                throw new NotImplementedException();

            return Task.FromResult(Secrets[name]);
        }

        public Task DeleteSecret(string name, CancellationToken cancellationToken)
        {
            Secrets.TryRemove(name, out _);
            return Task.CompletedTask;
        }
    }
}
