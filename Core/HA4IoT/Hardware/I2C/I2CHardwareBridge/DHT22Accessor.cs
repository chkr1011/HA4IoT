using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class DHT22Accessor
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, Dht22State> _dht22States = new Dictionary<int, Dht22State>();
        private readonly I2CHardwareBridge _i2CHardwareBridge;

        public DHT22Accessor(I2CHardwareBridge i2CHardwareBridge, ISchedulerService schedulerService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            
            _i2CHardwareBridge = i2CHardwareBridge ?? throw new ArgumentNullException(nameof(i2CHardwareBridge));

            schedulerService.Register("DHT22Updater", TimeSpan.FromSeconds(10), () => FetchValues());
        }

        public event EventHandler ValuesUpdated;

        public DHT22TemperatureSensor GetTemperatureSensor(int pin)
        {
            lock (_syncRoot)
            {
                if (!_dht22States.ContainsKey(pin))
                {
                    _dht22States[pin] = new Dht22State();
                }
            }
            
            return new DHT22TemperatureSensor(pin, this);
        }

        public float GetTemperature(byte pin)
        {
            lock (_syncRoot)
            {
                return _dht22States[pin].Temperature;
            }
        }

        public DHT22HumiditySensor GetHumiditySensor(byte pin)
        {
            lock (_syncRoot)
            {
                if (!_dht22States.ContainsKey(pin))
                {
                    _dht22States[pin] = new Dht22State();
                }
            }

            return new DHT22HumiditySensor(pin, this);
        }

        public float GetHumidity(byte pin)
        {
            lock (_syncRoot)
            {
                return _dht22States[pin].Humidity;
            }
        }

        private void FetchValues()
        {
            lock (_syncRoot)
            {
                foreach (var sensor in _dht22States)
                {
                    float temperature;
                    float humidity;
                    if (TryFetchValues(sensor.Key, out temperature, out humidity))
                    {
                        sensor.Value.Temperature = temperature;
                        sensor.Value.Humidity = humidity;
                    }

                }
            }

            ValuesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private bool TryFetchValues(int pin, out float temperature, out float humidity)
        {
            temperature = 0;
            humidity = 0;

            var command = new ReadDHT22SensorCommand().WithPin((byte)pin);
            _i2CHardwareBridge.ExecuteCommand(command);

            if (command.Response == null || !command.Response.Succeeded)
            {
                return false;
            }

            temperature = command.Response.Temperature;
            humidity = command.Response.Humidity;
            return true;
        }
    }
}
