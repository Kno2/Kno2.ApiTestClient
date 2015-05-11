using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kno2.ApiTestClient.Resources;

namespace Kno2.ApiTestClient.Helpers
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
            string ns = typeof(Program).Namespace;
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

        public static bool FileHashMatches(AttachmentResource attachment)
        {
            // Get the hash of the binary returned from the API
            byte[] attachmentFileHash = FileHash(attachment.NativeFileBytes);

            // Get the hash of the embedded resource samples
            string extension = Path.GetExtension(attachment.NativeFileName);
            var fileType = (FileType)Enum.Parse(typeof(FileType), extension.Substring(1), true);
            byte[] sampleAttachmentHash = GetSampleAttachmentHash(fileType);

            return sampleAttachmentHash.SequenceEqual(attachmentFileHash);
        }
    }

    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var descriptionAttributes = (DescriptionAttribute[])
                (value.GetType().GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false));
            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : value.ToString();
        }
    }
}