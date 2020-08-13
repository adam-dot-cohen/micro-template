using System.Collections.Generic;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure.AzureResources
{
    public static class StorageResourceNames
    {
        public static string GetEscrowContainerName(string partnerId)
        {
            return $"transfer-{partnerId}";
        }

        public static Dictionary<string, ProvisionedResourceType> GetDataResourceNames()
        {
            return new Dictionary<string, ProvisionedResourceType>
            {
                    {"raw", ProvisionedResourceType.RawFileSystemDirectory},
                    {"curated", ProvisionedResourceType.CuratedFileSystemDirectory},
                    {"rejected", ProvisionedResourceType.RejectedFileSystemDirectory},
                    {"published", ProvisionedResourceType.PublishedFileSystemDirectory},
                    {"experiment", ProvisionedResourceType.ExperimentFileSystemDirectory}
            };
        }
    }
}