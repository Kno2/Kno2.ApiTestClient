using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Kno2.ApiTestClient.Core;
using Kno2.ApiTestClient.Core.Helpers;

namespace Kno2.ApiTestClient.Send.Extensions
{
    public static class FileHelpers
    {
        /// <summary>
        /// Returns a sample document byte array from the sample application
        /// Make sure your samples are set to embedded resource
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static byte[] GetSampleAttachmentBytes(FileType fileType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string ns = typeof(Program).Namespace;
            string name = String.Format("{0}.Samples.Document.{1}", ns, fileType);
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null) return null;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Returns a sample document byte array from the sample application
        /// Make sure your samples are set to embedded resource
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static byte[] GetSampleAttachmentHash(FileType fileType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string ns = typeof(ApiConfig).Namespace;
            string name = String.Format("{0}.Samples.Document.{1}", ns, fileType);
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null) return null;
                return FileHash(stream);
            }
        }


        /// <summary>
        /// Simple file name generator for demonstration purposes only
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static string GenerateAttachmentName(FileType fileType)
        {
            return fileType.Description() + "-Document-" + Guid.NewGuid().ToString("N") + "." + fileType;
        }

        private static HashAlgorithm GetHashAlgorithm()
        {
            return SHA256.Create();
        }

        public static byte[] FileHash(Stream stream)
        {
            using (var hashAlgorithm = GetHashAlgorithm())
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }

        public static byte[] FileHash(byte[] bytes)
        {
            using (var hashAlgorithm = GetHashAlgorithm())
            {
                return hashAlgorithm.ComputeHash(bytes);
            }
        }

        public static byte[] FileHash(string fileName)
        {
            using (var hashAlgorithm = GetHashAlgorithm())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return hashAlgorithm.ComputeHash(stream);
                }
            }
        }
    }
}