﻿using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Laso.Identity.Api.Configuration
{
    internal static class Certificate
    {
        public static X509Certificate2 Get()
        {
            var assembly = typeof(Certificate).GetTypeInfo().Assembly;

            /***********************************************************************************************
             * For now, just using a local certificate for dev
             **********************************************************************************************/
            using (var stream = assembly.GetManifestResourceStream("Laso.Identity.Api.Certificates.laso_dev.pfx"))
            {
                return new X509Certificate2(ReadStream(stream), "lasodev!");
            }
        }

        private static byte[] ReadStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}