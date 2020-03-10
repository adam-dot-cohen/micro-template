using System;
using System.Threading;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Core
{
    public interface IApplicationSecrets
    {
        Task<KeyVaultSecret> GetSecret(string name, CancellationToken cancellationToken);
    }

    public class KeyVaultSecret
    {
        public string Value { get; set; }
        public string Version { get; set; }
        public Uri Reference { get; set; }
    }
}
