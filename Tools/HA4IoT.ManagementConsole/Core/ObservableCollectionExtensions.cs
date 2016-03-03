using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HA4IoT.ManagementConsole.Core
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<TITem>(this ObservableCollection<TITem> collection, IEnumerable<TITem> items)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
