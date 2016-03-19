using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Telemetry.History
{
    public class SensorActuatorHistory
    {
        private readonly object _syncRoot = new object();
        private readonly string _filename;
        private readonly ISingleValueSensorActuator _sensor;

        public SensorActuatorHistory(ISingleValueSensorActuator sensor)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            _sensor = sensor;

            _filename = StoragePath.WithFilename("Actuators", sensor.Id.Value, "History.csv");

            sensor.ValueChanged += CreateDataPointAsync;
        }

        public void Reset()
        {
            lock (_syncRoot)
            {
                if (!File.Exists(_filename))
                {
                    return;
                }

                File.Delete(_filename);
            }
        }

        public void ExposeToApi(IApiController apiController)
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            apiController.RouteRequest($"actuators/{_sensor.Id}/history", HandleApiRequest);
            apiController.RouteCommand($"actuators/{_sensor.Id}/history", HandleApiCommand);
        }

        private void HandleApiCommand(IApiContext apiContext)
        {
            Reset();
        }

        private void HandleApiRequest(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                var dataPoints = new JsonArray();

                if (File.Exists(_filename))
                {
                    using (var fileStream = File.OpenRead(_filename))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            string line = streamReader.ReadLine();
                            string[] columns = line.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            var dataPoint = new JsonObject();
                            dataPoint.SetNamedValue("Timestamp", JsonValue.CreateStringValue(columns[0]));
                            dataPoint.SetNamedValue("Value", JsonValue.CreateNumberValue(float.Parse(columns[1], NumberFormatInfo.InvariantInfo)));

                            dataPoints.Add(dataPoint);
                        }
                    }
                }
                
                apiContext.Response.SetNamedValue("History", dataPoints); 
            }
        }

        private async void CreateDataPointAsync(object sender, SingleValueSensorValueChangedEventArgs e)
        {
            await Task.Run(() => AppendDataPoint(e.NewValue));
        }

        private void AppendDataPoint(float value)
        {
            var dataPoint = new SensorDataPoint(DateTime.Now, value);

            lock (_syncRoot)
            {
                try
                {
                    File.AppendAllText(_filename, ConvertDataPointToCsvRow(dataPoint) + Environment.NewLine);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error while adding data point.");
                }
            }
        }

        private string ConvertDataPointToCsvRow(SensorDataPoint dataPoint)
        {
            return dataPoint.Timestamp.ToString("O") + "," + dataPoint.Value.ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}
