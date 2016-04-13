using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Telemetry
{
    public class ComponentStateHistoryTracker
    {
        private readonly object _syncRoot = new object();
        private readonly string _filename;
        private readonly IComponent _component;

        public ComponentStateHistoryTracker(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _component = component;

            _filename = StoragePath.WithFilename("Components", component.Id.Value, "History.csv");
            StoragePath.EnsureDirectoryExists(_filename);

            component.StateChanged += CreateDataPointAsync;
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

            apiController.RouteRequest($"component/{_component.Id}/history", HandleApiRequest);
            apiController.RouteCommand($"component/{_component.Id}/history", HandleApiCommand);
        }

        private void CreateDataPointAsync(object sender, ComponentStateChangedEventArgs e)
        {
            Task.Run(() => AppendDataPoint(e));
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

        private void AppendDataPoint(ComponentStateChangedEventArgs eventArgs)
        {
            string line = DateTime.Now.ToString("O") + "," + eventArgs.NewState.ToString() + Environment.NewLine;

            lock (_syncRoot)
            {
                try
                {
                    File.AppendAllText(_filename, line);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, $"Error while adding data point for component {_component.Id}.");
                }
            }
        }
    }
}
