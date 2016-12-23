using System.IO;

namespace HA4IoT.Contracts.Core
{
    public static class StoragePath
    {
        public static string Root { get; set; }

        public static string AppRoot { get; } = Path.Combine(Root, "App");

        public static string ManagementAppRoot { get; } = Path.Combine(Root, "ManagementApp");
    }
}
