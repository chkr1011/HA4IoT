using System;
using System.IO;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Storage;
using Newtonsoft.Json;

namespace HA4IoT.Services.StorageService
{
    public class StorageService : ServiceBase, IStorageService
    {
        private readonly string _rootPath = StoragePath.Root;

        public bool TryRead<TData>(string filename, out TData data)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            data = default(TData);
            try
            {
                var absoluteFilename = Path.Combine(_rootPath, filename);
                if (!File.Exists(absoluteFilename))
                {
                    return false;
                }

                var fileContent = File.ReadAllText(absoluteFilename);
                if (string.IsNullOrEmpty(fileContent))
                {
                    return false;
                }

                data = JsonConvert.DeserializeObject<TData>(fileContent);
                return true;
            }
            catch (Exception exception)
            {
                Log.Warning(exception, $"Unable to load data from '{filename}'.");
            }

            return false;
        }

        public void Write<TData>(string filename, TData content)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            var absoluteFilename = Path.Combine(_rootPath, filename);
            var json = JsonConvert.SerializeObject(content, Formatting.Indented);

            File.WriteAllText(absoluteFilename, json);
        }
    }
}
