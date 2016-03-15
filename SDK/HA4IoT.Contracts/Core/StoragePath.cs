using System.IO;
using Windows.Storage;

namespace HA4IoT.Contracts.Core
{
    public class StoragePath
    {
        public static string Root => ApplicationData.Current.LocalFolder.Path;

        public static string WithFilename(params string[] paths)
        {
            return Path.Combine(Root, Path.Combine(paths));
        }
    }
}
