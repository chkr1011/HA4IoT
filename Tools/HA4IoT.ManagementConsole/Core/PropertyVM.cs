using System;

namespace HA4IoT.ManagementConsole.Core
{
    public class PropertyVM<TValue> : ViewModelBase
    {
        private TValue _value;

        public PropertyVM(TValue initialValue)
        {
            Value = initialValue;
        }

        public TValue Value
        {
            get { return _value; }

            set
            {
                _value = value;
                OnPropertyChangedFromCaller();
            }
        }

        public override string ToString()
        {
            return Convert.ToString(_value);
        }
    }
}
