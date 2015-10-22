using System;
using System.Collections.Generic;
using HA4IoT.Core.Timer;
using HA4IoT.Hardware.I2CHardwareBridge;

namespace HA4IoT.Hardware.DHT22
{
    public class DHT22Accessor
    {
        private readonly I2CHardwareBridge.I2CHardwareBridge _i2CHardwareBridge;

        private readonly HashSet<byte> _openPins = new HashSet<byte>();
        private readonly Dictionary<byte, float> _humidities = new Dictionary<byte, float>();
        private readonly Dictionary<byte, float> _temperatures = new Dictionary<byte, float>();
        
        public DHT22Accessor(I2CHardwareBridge.I2CHardwareBridge i2CHardwareBridge, IHomeAutomationTimer timer)
        {
            if (i2CHardwareBridge == null) throw new ArgumentNullException(nameof(i2CHardwareBridge));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _i2CHardwareBridge = i2CHardwareBridge;
            timer.Every(TimeSpan.FromSeconds(10)).Do(FetchValues);
        }

        public event EventHandler ValuesUpdated;

        public DHT22TemperatureSensor GetTemperatureSensor(byte pin)
        {
            _openPins.Add(pin);
            _temperatures[pin] = 0.0F;

            return new DHT22TemperatureSensor(pin, this);
        }

        public float GetTemperature(byte pin)
        {
            return _temperatures[pin];
        }

        public DHT22HumiditySensor GetHumiditySensor(byte pin)
        {
            _openPins.Add(pin);
            _humidities[pin] = 0.0F;

            return new DHT22HumiditySensor(pin, this);
        }

        public float GetHumidity(byte pin)
        {
            return _humidities[pin];
        }

        private void FetchValues()
        {
            foreach (var openPin in _openPins)
            {
                FetchValues(openPin);
            }
            
            ValuesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void FetchValues(byte pin)
        {
            var command = new ReadDHT22SensorCommand().WithPin(pin);
            _i2CHardwareBridge.ExecuteCommand(command);

            if (command.Response != null && command.Response.Succeeded)
            {
                _temperatures[pin] = command.Response.Temperature;
                _humidities[pin] = command.Response.Humidity;
            }
        }
    }
}
