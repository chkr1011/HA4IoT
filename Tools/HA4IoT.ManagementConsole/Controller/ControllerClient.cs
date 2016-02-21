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
            return await GetJObject("configuration");
        }

        public async Task SetAutomationConfiguration(string automationId, JObject configuration)
        {
            string relativeUrl = "automation/" + automationId + "/settings";
            await PostJObject(relativeUrl, configuration);
        }

        public async Task SetActuatorConfiguration(string actuatorId, JObject configuration)
        {
            string relativeUrl = "actuator/" + actuatorId + "/settings";
            await PostJObject(relativeUrl, configuration);
        }

        public async Task<JObject> GetStatus()
        {
            return await GetJObject("status");
        }

        public async Task<JObject> GetHealth()
        {
            return await GetJObject("health");
        }

        public async Task<JObject> GetLog()
        {
            return await GetJObject("notifications");
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
            return new Uri("http://" + _address + ":80/api/" + relativePath);
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
