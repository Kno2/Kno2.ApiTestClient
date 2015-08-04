using System;
using System.IO;
using System.Reflection;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public static class FileIoExtensions
    {
        public static string AsAppPath(this string filePath, bool appNamePrefix = true)
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrWhiteSpace(directoryName)) return filePath;

            if (appNamePrefix)
            {
                string appName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(directoryName, appName + "-" + filePath);
            }

            return Path.Combine(directoryName, filePath);
            
        }
    }
}