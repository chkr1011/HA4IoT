using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Storage;

namespace HA4IoT.Tests.Mockups.Services
{
    public class TestStorageService : ServiceBase, IStorageService
    {
        private readonly Dictionary<string, object> _files = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        
        public bool TryRead<TData>(string filename, out TData data)
        {
            object buffer;
            if (!_files.TryGetValue(filename, out buffer))
            {
                data = default(TData);
                return false;
            }

            data = (TData)buffer;
            return true;
        }

        public void Write<TData>(string filename, TData content)
        {
            _files[filename] = content;
        }
    }
}
