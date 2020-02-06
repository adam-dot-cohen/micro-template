using Microsoft.AspNetCore.StaticFiles;

namespace Partner.Core.IO.File
{
    public static class MimeTypes
    {
        private static readonly FileExtensionContentTypeProvider Mapper = new FileExtensionContentTypeProvider();

        /// <summary>
        /// Given a file path, determine the MIME type. Defaults to application/octet-stream
        /// </summary>
        public static string GetContentType(string subpath)
        {
            if (!Mapper.TryGetContentType(subpath, out var contentType))
                return "application/octet-stream";

            return contentType;
        }
    }
}
