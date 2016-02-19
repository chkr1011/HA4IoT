using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Networking;

namespace HA4IoT.Core.Settings
{
    public class Setting<TValue> : IExportToJsonValue, IImportFromJsonValue
    {
        private TValue _value;
        
        private IJsonValue _serializedValue;

        public Setting(TValue defaultValue)
        {
            DefaultValue = defaultValue;
            _serializedValue = DefaultValue.ToJsonValue();
        }

        public event EventHandler<ValueChangedEventArgsBase<TValue>> ValueChanged;

        public TValue Value
        {
            get
            {
                if (!IsValueSet)
                {
                    return DefaultValue;
                }

                return _value;
            }

            set
            {
                TValue oldValue = Value;

                _value = value;
                IsValueSet = true;

                _serializedValue = _value.ToJsonValue();

                ValueChanged?.Invoke(this, new ValueChangedEventArgsBase<TValue>(oldValue, _value));
            }
        }

        public TValue DefaultValue { get; set; }

        public bool IsValueSet { get; private set; }

        public static implicit operator TValue(Setting<TValue> setting)
        {
            return setting.Value;
        }

        public IJsonValue ExportToJsonObject()
        {
            return _serializedValue;
        }

        public void ImportFromJsonValue(IJsonValue value)
        {
            Value = value.ToObject<TValue>();
        }
    }
}
