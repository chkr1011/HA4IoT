using System;
using System.Collections;
using System.Collections.Generic;

namespace HA4IoT.Logging
{
    public class RollingCollection<TItem> : IEnumerable<TItem>
    {
        private readonly TItem[] _items;

        private int _index;

        public RollingCollection(int count)
        {
            _items = new TItem[count];
        }

        public ulong Version { get; private set; }

        public int Count { get; private set; }

        public void Add(TItem item)
        {
            if (_index >= _items.Length)
            {
                for (var i = 1; i < _items.Length; i++)
                {
                    _items[i - 1] = _items[i];
                }

                _items[_items.Length - 1] = item;
            }
            else
            {
                _items[_index] = item;
                Count++;
                _index++;
            }
            
            Version++;
        }

        public TItem this[int index] => _items[index];

        public void Clear()
        {
            _index = 0;
            Count = 0;
            
            for (var i = 0; i < _items.Length; i++)
            {
                _items[i] = default(TItem);
            }

            Version++;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return new RollingCollectionEnumerator<TItem>(this, Version);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RollingCollectionEnumerator<TItem> : IEnumerator<TItem>
    {
        private readonly RollingCollection<TItem> _source;
        private readonly ulong _initialVersion;

        private int _index;

        public RollingCollectionEnumerator(RollingCollection<TItem> source, ulong initialVersion)
        {
            _source = source;
            _initialVersion = initialVersion;
        }

        public TItem Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_source.Version != _initialVersion)
            {
                throw new InvalidOperationException("Rolling collection was modified.");
            }

            if (_index >= _source.Count)
            {
                return false;
            }

            Current = _source[_index];
            _index++;
            return true;
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
        }
    }
}
