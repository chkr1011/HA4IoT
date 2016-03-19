using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Controller
{
    public class ControllerClient
    {
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);
        private readonly HttpClient _httpClient = new HttpClient();

        private string _address;
        private bool _isWorking;

        public event EventHandler IsWorkingChanged;
        
        public string Address
        {
            get { return _address; }

            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _address = value;
            }
        }

        public bool IsWorking
        {
            get { return _isWorking; }

            private set
            {
                _isWorking = value;
                IsWorkingChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public async Task<JObject> GetConfiguration()
        {
            return await GetJObject("api/configuration");
        }

        public async Task PostAutomationConfiguration(string automationId, JObject configuration)
        {
            if (automationId == null) throw new ArgumentNullException(nameof(automationId));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            string relativeUrl = $"api/area/{automationId}/settings";
            await PostJObject(relativeUrl, configuration);
        }

        public async Task PostActuatorConfiguration(string actuatorId, JObject configuration)
        {
            if (actuatorId == null) throw new ArgumentNullException(nameof(actuatorId));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            string relativeUrl = $"api/area/{actuatorId}/settings";
            await PostJObject(relativeUrl, configuration);
        }

        public async Task PostAreaConfiguration(string areaId, JObject configuration)
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            string relativeUrl = $"api/area/{areaId}/settings";
            await PostJObject(relativeUrl, configuration);
        }

        public async Task<JObject> GetStatus()
        {
            return await GetJObject("api/status");
        }

        public async Task<JObject> GetHealth()
        {
            return await GetJObject("api/health");
        }

        public async Task<JObject> GetLog()
        {
            return await GetJObject("api/notifications");
        }

        public async Task<JObject> GetStorageJsonFile(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            return await GetJObject($"storage/{filename}");
        }

        public async Task PostStorageJsonFile(string filename, JObject content)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            if (content == null) throw new ArgumentNullException(nameof(content));

            await PostJObject($"storage/{filename}", content);
        }

        private async Task<JObject> GetJObject(string relativeUrl)
        {
            ThrowIfAddressInvalid();

            Uri uri = GenerateUri(relativeUrl);

            string response;
            try
            {
                await _syncRoot.WaitAsync();

                IsWorking = true;
                response = await _httpClient.GetStringAsync(uri);
            }
            finally
            {
                IsWorking = false;
                _syncRoot.Release();
            }

            return JObject.Parse(response);
        }

        private async Task PostJObject(string relativeUrl, JObject body)
        {
            ThrowIfAddressInvalid();

            Uri uri = GenerateUri(relativeUrl);

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                request.Content = new StringContent(body.ToString(), Encoding.UTF8);

                try
                {
                    await _syncRoot.WaitAsync();

                    IsWorking = true;
                    using (var response = await _httpClient.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new InvalidOperationException("POST failed. Code=" + response.StatusCode);
                        }
                    }
                }
                finally
                {
                    IsWorking = false;
                    _syncRoot.Release();
                }
            }
        }

        private Uri GenerateUri(string relativePath)
        {
            return new Uri("http://" + _address + ":80/" + relativePath);
        }

        private void ThrowIfAddressInvalid()
        {
            if (string.IsNullOrWhiteSpace(_address))
            {
                throw new InvalidOperationException("Address not set");
            }
        }
    }
}
