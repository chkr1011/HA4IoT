using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HA4IoT.ManagementConsole.Core
{
    public class SelectableObservableCollection<TItem> : ObservableCollection<TItem>
    {
        private TItem _selectedItem;

        public event EventHandler SelectedItemChanged;

        public TItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;

                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged(new PropertyChangedEventArgs("SelectedItem"));
            }
        }

        public void NotifyCommandIfSelectionChanged(ICheckCanExecute command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            SelectedItemChanged += (s, e) => command.CheckCanExecute();
        }

        public void AddRange(IEnumerable<TItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                Add(item);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (_selectedItem != null)
            {
                bool selectedItemIsRemoved = IndexOf(_selectedItem) == index;
                if (selectedItemIsRemoved)
                {
                    SelectedItem = default(TItem);
                }
            }

            base.RemoveItem(index);
        }

        public void MoveItemUp(TItem item)
        {
            int itemIndex = IndexOf(item);
            if (itemIndex == 0)
            {
                return;
            }

            Move(itemIndex, itemIndex - 1);
        }

        public void MoveItemDown(TItem item)
        {
            int itemIndex = IndexOf(item);
            if (itemIndex == Count - 1)
            {
                return;
            }

            Move(itemIndex, itemIndex + 1);
        }
    }
}
