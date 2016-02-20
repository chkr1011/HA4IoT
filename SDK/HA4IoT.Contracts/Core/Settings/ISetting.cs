using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Networking;

namespace HA4IoT.Contracts.Core.Settings
{
    public interface ISetting<TValue> : IImportFromJsonValue, IExportToJsonValue
    {
        event EventHandler<ValueChangedEventArgs<TValue>> ValueChanged;

        TValue DefaultValue { get; set; }

        bool IsValueSet { get; }

        TValue Value { get; set; }
    }
}