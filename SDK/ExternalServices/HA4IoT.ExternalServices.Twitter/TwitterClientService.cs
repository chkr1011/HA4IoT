using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.ExternalServices.Twitter;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.ExternalServices.Twitter
{
    public class TwitterClientService : ServiceBase, ITwitterClientService
    {
        private string _nonce;
        private string _timestamp;

        public TwitterClientService(ISettingsService settingsService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            settingsService.CreateSettingsMonitor<TwitterClientServiceSettings>(s => Settings = s);
        }

        public TwitterClientServiceSettings Settings { get; private set; }

        public async Task Tweet(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            if (!Settings.IsEnabled)
            {
                Log.Verbose("Twitter client service is disabled.");
                return;
            }
            
            _nonce = GetNonce();
            _timestamp = GetTimeStamp();

            var signature = GetSignatureForRequest(message);
            var oAuthToken = GetAuthorizationToken(signature);
                
            using (var httpClient = new HttpClient())
            {
                string url = "https://api.twitter.com/1.1/statuses/update.json?status=" + Uri.EscapeDataString(message);

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", oAuthToken);
         
                var response = await httpClient.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException(response.StatusCode.ToString());
                }
            }
        }

        private string GetSignatureForRequest(string message)
        {
            var parameters = new List<string>
            {
                $"oauth_consumer_key={Uri.EscapeDataString(Settings.ConsumerKey)}",
                $"oauth_nonce={_nonce}",
                "oauth_signature_method=HMAC-SHA1",
                $"oauth_timestamp={_timestamp}",
                $"oauth_token={Uri.EscapeDataString(Settings.AccessToken)}",
                "oauth_version=1.0",
                $"status={Uri.EscapeDataString(message)}"
            };

            var parametersString = Uri.EscapeDataString(string.Join("&", parameters));
            var url = Uri.EscapeDataString("https://api.twitter.com/1.1/statuses/update.json");

            var signingContent = string.Format(
                "{0}&{1}&{2}",
                "POST",
                url,
                parametersString);

            return GenerateSignature(signingContent);
        }

        private string GetAuthorizationToken(string signature)
        {
            var values = new List<string>
            {
                $"oauth_consumer_key=\"{Uri.EscapeDataString(Settings.ConsumerKey)}\"",
                $"oauth_nonce=\"{_nonce}\"",
                $"oauth_signature=\"{Uri.EscapeDataString(signature)}\"",
                "oauth_signature_method=\"HMAC-SHA1\"",
                $"oauth_timestamp=\"{_timestamp}\"",
                $"oauth_token=\"{Uri.EscapeDataString(Settings.AccessToken)}\"",
                "oauth_version=\"1.0\""
            };

            return "OAuth " + string.Join(", ", values);
        }

        private string GetNonce()
        {
            return Uri.EscapeDataString(Guid.NewGuid().ToString());
        }

        private string GetTimeStamp()
        {
            var sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Math.Round(sinceEpoch.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private string GenerateSignature(string content)
        {
            var key = Uri.EscapeDataString(Settings.ConsumerSecret) + "&" + Uri.EscapeDataString(Settings.AccessTokenSecret);

            var keyMaterial = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            var macAlgorithm = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            var macKey = macAlgorithm.CreateKey(keyMaterial);
            
            var buffer = CryptographicBuffer.ConvertStringToBinary(content, BinaryStringEncoding.Utf8);

            var signatureBuffer = CryptographicEngine.Sign(macKey, buffer);
            var signature = CryptographicBuffer.EncodeToBase64String(signatureBuffer);

            return signature;
        }
    }
}
