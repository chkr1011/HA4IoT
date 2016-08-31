using System;
using System.IO;
using Windows.Storage;

namespace HA4IoT.Contracts.Core
{
    public class StoragePath
    {
        public static string Root { get; set; } = ApplicationData.Current.LocalFolder.Path;

        public static string AppRoot { get; set; } = WithFilename("App");

        public static string ManagementAppRoot { get; set; } = WithFilename("ManagementApp");

        public static string WithFilename(params string[] paths)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));

            return Path.Combine(Root, Path.Combine(paths));
        }

        public static void EnsureDirectoryExists(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            string path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
