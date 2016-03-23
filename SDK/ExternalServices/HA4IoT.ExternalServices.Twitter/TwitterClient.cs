using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Services;

namespace HA4IoT.ExternalServices.Twitter
{
    public class TwitterClient : IService
    {
        private string _nonce;
        private string _timestamp;

        public string AccessTokenSecret { get; set; }
        public string AccessToken { get; set; }
        public string CosumerSecret { get; set; }
        public string ConsumerKey { get; set; }

        public async Task Tweet(string message)
        {
            _nonce = GetNonce();
            _timestamp = GetTimeStamp();

            var signature = GetSignatureForRequest(message);
            var oAuthToken = GetAuthorizationToken(signature);
                
            using (var httpClient = new HttpClient())
            {
                string url = "https://api.twitter.com/1.1/statuses/update.json?status=" + Uri.EscapeDataString(message);

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", oAuthToken);
                request.Version = new Version(1, 1);

                var response = await httpClient.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException(response.StatusCode.ToString());
                }
            }
        }

        public IHomeAutomationAction GetTweetAction(string message)
        {
            return new TweetAction(message, this);
        }

        public IHomeAutomationAction GetTweetAction(Func<string> messageProvider)
        {
            return new TweetAction(messageProvider, this);
        }

        private string GetAuthorizationToken(string signature)
        {
            var values = new List<string>();
            values.Add($"oauth_consumer_key=\"{Uri.EscapeDataString(ConsumerKey)}\"");
            values.Add($"oauth_nonce=\"{_nonce}\"");
            values.Add($"oauth_signature=\"{Uri.EscapeDataString(signature)}\"");
            values.Add("oauth_signature_method=\"HMAC-SHA1\"");
            values.Add($"oauth_timestamp=\"{_timestamp}\"");
            values.Add($"oauth_token=\"{Uri.EscapeDataString(AccessToken)}\"");
            values.Add("oauth_version=\"1.0\"");

            return "OAuth " + string.Join(", ", values);
        }

        private string GetSignatureForRequest(string message)
        {
            var parameters = new List<string>();
            parameters.Add($"oauth_consumer_key={Uri.EscapeDataString(ConsumerKey)}");
            parameters.Add($"oauth_nonce={_nonce}");
            parameters.Add("oauth_signature_method=HMAC-SHA1");
            parameters.Add($"oauth_timestamp={_timestamp}");
            parameters.Add($"oauth_token={Uri.EscapeDataString(AccessToken)}");
            parameters.Add("oauth_version=1.0");
            parameters.Add($"status={Uri.EscapeDataString(message)}");

            var parametersString = Uri.EscapeDataString(string.Join("&", parameters));
            var url = Uri.EscapeDataString("https://api.twitter.com/1.1/statuses/update.json");

            var signingContent = string.Format(
                "{0}&{1}&{2}",
                "POST",
                url,
                parametersString);

            return GenerateSignature(signingContent);
        }

        private string GetNonce()
        {
            return Uri.EscapeDataString(Guid.NewGuid().ToString());
        }

        private string GetTimeStamp()
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Math.Round(sinceEpoch.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private string GenerateSignature(string content)
        {
            string key = Uri.EscapeDataString(CosumerSecret) + "&" + Uri.EscapeDataString(AccessTokenSecret);

            IBuffer keyMaterial = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            MacAlgorithmProvider macAlgorithm = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            CryptographicKey macKey = macAlgorithm.CreateKey(keyMaterial);
            
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(content, BinaryStringEncoding.Utf8);

            IBuffer signatureBuffer = CryptographicEngine.Sign(macKey, buffer);
            string signature = CryptographicBuffer.EncodeToBase64String(signatureBuffer);

            return signature;
        }
    }
}
