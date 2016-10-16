using System;
using System.IO;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Telemetry
{
    public class ComponentStateHistoryTracker
    {
        private static readonly char[] CsvSeparator = {','};
        private readonly object _syncRoot = new object();
        //private readonly string _filename;
        private readonly IComponent _component;

        public ComponentStateHistoryTracker(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _component = component;

            //_filename = StoragePath.WithFilename("Components", component.Id.Value, "History.csv");
            //StoragePath.EnsureDirectoryExists(_filename);

            component.StateChanged += CreateDataPointAsync;
        }

        public void Reset()
        {
            ////lock (_syncRoot)
            ////{
            ////    if (!File.Exists(_filename))
            ////    {
            ////        return;
            ////    }

            ////    File.Delete(_filename);
            ////}
        }

        private void CreateDataPointAsync(object sender, ComponentStateChangedEventArgs e)
        {
            //Task.Run(() => AppendDataPoint(e));
        }

        private void HandleApiCommand(IApiContext apiContext)
        {
            Reset();
        }

        private void HandleApiRequest(IApiContext apiContext)
        {
            ////var dataPoints = new JArray();
            ////long fileSize = 0;

            ////lock (_syncRoot)
            ////{
            ////    var fileInfo = new FileInfo(_filename);
            ////    if (fileInfo.Exists)
            ////    {
            ////        fileSize = fileInfo.Length;
                    
            ////        using (var fileStream = fileInfo.OpenRead())
            ////        using (var streamReader = new StreamReader(fileStream))
            ////        {
            ////            while (!streamReader.EndOfStream)
            ////            {
            ////                string line = streamReader.ReadLine();
            ////                dataPoints.Add(ConvertCsvLineToJsonObject(line));
            ////            }
            ////        }
            ////    }
            ////}

            ////apiContext.Response.SetNamedValue("history", dataPoints);
            ////apiContext.Response.SetValue("fileSize", fileSize);
        }

        private JObject ConvertCsvLineToJsonObject(string line)
        {
            var columns = line.Split(CsvSeparator, StringSplitOptions.RemoveEmptyEntries);

            var dataPoint = new JObject
            {
                ["timestamp"] = columns[0],
                ["state"] = columns[1]
            };

            return dataPoint;
        }

        private void AppendDataPoint(ComponentStateChangedEventArgs eventArgs)
        {
            ////string line = DateTime.Now.ToString("O") + "," + eventArgs.NewState.ToString() + Environment.NewLine;

            ////lock (_syncRoot)
            ////{
            ////    try
            ////    {
            ////        File.AppendAllText(_filename, line);
            ////    }
            ////    catch (Exception exception)
            ////    {
            ////        Log.Error(exception, $"Error while adding data point for component {_component.Id}.");
            ////    }
            ////}
        }
    }
}
