using System;
using System.IO;
using DataImport.Core.IO.File;

namespace DataImport.Services.IO.Storage.Blob.Azure
{
    public enum StorageType
    {
        Http,
        Blob,
        File
    }

    public class StorageMoniker
    {
        public StorageType Type { get; set; }
        public string MimeType { get; set; }
        public string Authority { get; set; }
        public string LocalPath { get; set; }
        public string FileName { get; set; }

        private static readonly char PathSeparator = '/';

        public static StorageMoniker Parse(string moniker)
        {
            var uri = new Uri(moniker);
            if (!Enum.TryParse(uri.Scheme, true, out StorageType type))
            {
                throw new ArgumentException($"Unknown storage scheme: '{uri.Scheme}'");
            }

            var fileName = Path.GetFileName(uri.AbsolutePath);
            var contentType = MimeTypes.GetContentType(uri.LocalPath.TrimStart(PathSeparator));

            return new StorageMoniker()
            {
                Type = type,
                Authority = uri.Authority,
                LocalPath = uri.LocalPath,
                FileName = fileName,
                MimeType = contentType,
            };
        }

        public static bool TryParse(string moniker, out StorageMoniker storageMoniker)
        {
            storageMoniker = null;

            try
            {
                storageMoniker = Parse(moniker);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
