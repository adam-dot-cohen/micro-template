using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Laso.DataImport.Services.Security
{
    public interface ISecureStore
    {
        Task<string> SetSecretAsync(string name, string value, CancellationToken cancellationToken);
        Task<string> GetSecretAsync(string name, string version = null, CancellationToken cancellationToken = default);
        Task<string> GetSecretOrDefaultAsync(string name, string version = null, string defaultValue = default, CancellationToken cancellationToken = default);
        Task<(string Value, string Version)> GetCurrentSecretAsync(string name, CancellationToken cancellationToken = default);

        Task<string> DecryptAsync(byte[] cipher, string certificateName, string version = null);
        Task<byte[]> GetPublicCertificateAsync(string certificateName, string version = null);
        Task<byte[]> GetPrivateCertificateAsync(string certificateName, string version = null);
    }
}
