using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Core;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure
{
    public class ResourceLocator : IResourceLocator
    {
        private readonly IApplicationSecrets _applicationSecrets;

        public ResourceLocator(IApplicationSecrets applicationSecrets)
        {
            _applicationSecrets = applicationSecrets;
        }

        public Task<string> GetLocationString(ProvisionedResourceEvent provisionedResource)
        {
            if (ResourceLocations.GetParentLocationByType(provisionedResource.Type) != ResourceLocations.PARTNERSECRETS) 
                return Task.FromResult(provisionedResource.Location);

            var getSecretTask = _applicationSecrets.GetSecret(provisionedResource.Location,
                CancellationToken.None);
            return getSecretTask.ContinueWith(st => st.Result);
        }

    }

    //TODO: this really ought to be moved into persisted state and cached or read-optimized
    public static class ResourceLocations
    {
        public const string PARTNERSECRETS = "PartnerSecrets";
        public const string ESCROWSTORAGE = "EscrowStorage";
        public const string COLDSTORAGE = "ColdStorage";
        public const string LASO_PGP_PUBLIC_KEY_FORMAT = "{0}-laso-pgp-publickey";
        public const string LASO_PGP_PRIVATE_KEY_FORMAT = "{0}-laso-pgp-privatekey";
        public const string LASO_PGP_PASSPHRASE_FORMAT = "{0}-laso-pgp-passphrase";
        public const string SFTP_USERNAME_FORMAT = "{0}-partner-ftp-username";
        public const string SFTP_PASSWORD_FORMAT = "{0}-partner-ftp-password";
        public const string PARTNER_PGP_PUBLIC_KEY_FORMAT = "{0}";

        public static string GetParentLocationByType(ProvisionedResourceType resourceType)
        {
            switch (resourceType)
            {
                case ProvisionedResourceType.EscrowIncoming:
                case ProvisionedResourceType.EscrowOutgoing:
                case ProvisionedResourceType.EscrowStorage:
                    return ResourceLocations.ESCROWSTORAGE;
                case ProvisionedResourceType.ColdStorage:
                    return ResourceLocations.COLDSTORAGE;
                case ProvisionedResourceType.RawFileSystemDirectory:
                    return "raw";
                case ProvisionedResourceType.SFTPAccount:
                    return "sFTP";
                case ProvisionedResourceType.SFTPUsername:
                case ProvisionedResourceType.SFTPPassword:
                case ProvisionedResourceType.LasoPGPPublicKey:
                case ProvisionedResourceType.LasoPGPPrivateKey:
                case ProvisionedResourceType.LasoPGPPassphrase:
                case ProvisionedResourceType.PartnerPGPPublicKey:
                    return ResourceLocations.PARTNERSECRETS;
                case ProvisionedResourceType.CuratedFileSystemDirectory:
                    return "curated";
                case ProvisionedResourceType.RejectedFileSystemDirectory:
                    return "rejected";
                case ProvisionedResourceType.PublishedFileSystemDirectory:
                    return "published";
                case ProvisionedResourceType.ExperimentFileSystemDirectory:
                    return "experiment";
                default:
                    throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null);
            }
        }
    }
}
