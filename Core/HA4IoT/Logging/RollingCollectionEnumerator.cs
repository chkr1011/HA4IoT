using System;
using System.Collections;
using System.Collections.Generic;

namespace HA4IoT.Logging
{
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