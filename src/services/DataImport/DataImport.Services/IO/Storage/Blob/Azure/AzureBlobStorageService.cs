using System;
using Laso.DataImport.Core.Configuration;
using Laso.DataImport.Core.IO;
using Laso.DataImport.Core.IO.File;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Services.Encryption;

namespace Laso.DataImport.Services.IO.Storage.Blob.Azure
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly Lazy<CloudBlobClient> _client;

        public AzureBlobStorageService(IConnectionStringConfiguration config)
        {
            _client = new Lazy<CloudBlobClient>(() => InitializeCloudBlobClient(config.LasoBlobStorageConnectionString));
        }

        public bool Exists(string container, string blobName)
        {
            return Exists(GetBlob(container, blobName));
        }

        private bool Exists(CloudBlockBlob blob)
        {
            return blob.Exists();
        }

        public StreamStack OpenRead(string container, string blobName)
        {
            var blob = GetBlob(container, blobName);
            if (!blob.Exists())
                return null;

            var streamStack = new StreamStack(blob.OpenRead());

            var isEncrypted = blob.Metadata.Get(BlobMetaDataKey.Encrypted).TryParseNullableBoolean();
            if (isEncrypted == true)
                throw new NotImplementedException("Blob decryption");

            var isCompressed = blob.Metadata.Get(BlobMetaDataKey.Compressed).TryParseNullableBoolean();

            if (isCompressed == true)
                streamStack.Decompress();

            return streamStack;
        }

        public void Delete(string container, string name)
        {
            var blob = GetBlob(container, name);

            if (Exists(blob))
                blob.Delete();
        }

        public StreamStack OpenWrite(string container, string name, string fileName = null, long? length = null, bool compress = false)
        {
            var blob = GetBlob(container, name);
            if (fileName.IsNotNullOrEmpty())
                blob.Metadata.Add(BlobMetaDataKey.FileName, fileName);

            if (length != null)
                blob.Metadata.Add(BlobMetaDataKey.Length, length.ToString());

            blob.Properties.ContentType = MimeTypes.GetContentType(blob.Name);

            var streamStack = new StreamStack(blob.OpenWrite());

            blob.Metadata.Add(BlobMetaDataKey.Compressed, compress.ToString());

            if (compress)
                streamStack.Compress();

            return streamStack;
        }

        private CloudBlobContainer GetContainer(string name)
        {
            var container = _client.Value.GetContainerReference(name.ToLower());

            // Workaround for bug in storage client
            if (!container.Exists())
                container.CreateIfNotExists();

            return container;
        }

        private CloudBlockBlob GetBlob(string containerName, string blobName)
        {
            var container = GetContainer(containerName);
            return container.GetBlockBlobReference(blobName);
        }

        private static CloudBlobClient InitializeCloudBlobClient(string connectionString)
        {
            var cloudBlobClient = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();

            // Disable MD5 validation via HTTPS. #6894
            if (cloudBlobClient.BaseUri.Scheme == Uri.UriSchemeHttps)
                cloudBlobClient.DefaultRequestOptions.DisableContentMD5Validation = true;

            return cloudBlobClient;
        }

        private static class BlobMetaDataKey
        {
            public const string Encrypted = "Encrypted";
            public const string EncryptionType = "EncryptionType";
            public const string Compressed = "Compressed";
            public const string FileName = "FileName";
            public const string Length = "Length";
            public const string Iv = "IV";
        }
    }
}
