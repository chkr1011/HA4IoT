using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Server;
using Newtonsoft.Json;

namespace HA4IoT.Devices
{
    internal class Storage : IMqttServerStorage
    {
        private readonly string _filename = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Retained.json");

        public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
        {
            try
            {
                var json = JsonConvert.SerializeObject(messages);
                File.WriteAllText(_filename, json);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return Task.CompletedTask;
        }

        public async Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
        {
            await Task.CompletedTask;

            if (!File.Exists(_filename))
            {
                return new List<MqttApplicationMessage>();
            }

            try
            {
                var json = File.ReadAllText(_filename);
                return JsonConvert.DeserializeObject<List<MqttApplicationMessage>>(json);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return new List<MqttApplicationMessage>();
            }
        }
    }
}
