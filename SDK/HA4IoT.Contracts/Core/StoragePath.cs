using System.IO;
using Windows.Storage;

namespace HA4IoT.Contracts.Core
{
    public static class StoragePath
    {
        public static string Root { get; } = ApplicationData.Current.LocalFolder.Path;

        public static string AppRoot { get; } = Path.Combine(Root, "App");

        public static string ManagementAppRoot { get; } = Path.Combine(Root, "ManagementApp");
    }
}
