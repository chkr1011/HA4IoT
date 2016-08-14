using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Services
{
    public class GenericControllerCollection<TId, TItem> where TId : IdBase
    {
        private readonly Dictionary<string, TItem> _items = new Dictionary<string, TItem>(StringComparer.OrdinalIgnoreCase);

        public void AddUnique(TId id, TItem item)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (_items.ContainsKey(id.Value))
            {
                throw new InvalidOperationException($"'{id}' aready registered.");
            }

            _items.Add(id.Value, item);
        }
        
        public void AddOrUpdate(TId id, TItem device)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (device == null) throw new ArgumentNullException(nameof(device));
            
            _items[id.Value] = device;
        }

        public TResult Get<TResult>() where TResult : TItem
        {
            return (TResult)_items.Values.Single(d => d is TResult);
        }

        public TItem Get(TId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            TItem item;
            if (!_items.TryGetValue(id.Value, out item))
            {
                throw new InvalidOperationException($"'{id}' not registered.");
            }

            return item;
        }

        public TResult Get<TResult>(TId id) where TResult : TItem
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            TItem item = Get(id);

            if (!(item is TResult))
            {
                throw new InvalidOperationException($"'{id}' is registered but no '{typeof(TResult).Name}' (is '{item.GetType().Name}').");
            }

            return (TResult)item;
        }

        public IList<TResult> GetAll<TResult>() where TResult : TItem
        {
            return _items.Values.OfType<TResult>().ToList();
        }

        public IList<TItem> GetAll()
        {
            return _items.Values.ToList();
        }

        public bool Contains(TId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _items.Any(i => i.Key.Equals(id.Value));
        }
    }
}