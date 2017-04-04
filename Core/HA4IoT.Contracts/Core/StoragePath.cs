using System.IO;

namespace HA4IoT.Contracts.Core
{
    public static class StoragePath
    {
        public static void Initialize(string storageRoot, string localStateRoot)
        {
            StorageRoot = storageRoot;
            AppRoot = Path.Combine(localStateRoot, "App");
            ManagementAppRoot = Path.Combine(localStateRoot, "ManagementApp");
        }

        public static string StorageRoot { get; private set; }

        public static string AppRoot { get; private set; } 

        public static string ManagementAppRoot { get; private set; }
    }
}
