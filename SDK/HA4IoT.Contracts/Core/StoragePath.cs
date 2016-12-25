using System.IO;

namespace HA4IoT.Contracts.Core
{
    public static class StoragePath
    {
        public static void Initialize(string root)
        {
            Root = root;
            AppRoot = Path.Combine(Root, "App");
            ManagementAppRoot = Path.Combine(Root, "ManagementApp");
        }

        public static string Root { get; private set; }

        public static string AppRoot { get; private set; }

        public static string ManagementAppRoot { get; private set; }
    }
}
