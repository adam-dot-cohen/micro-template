using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace Partner.Services.IO.Storage
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
            var mimeMapper = new FileExtensionContentTypeProvider();

            if (!mimeMapper.TryGetContentType(uri.LocalPath.TrimStart(PathSeparator), out var contentType))
            {
                contentType = "application/octet-stream";
            }

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
