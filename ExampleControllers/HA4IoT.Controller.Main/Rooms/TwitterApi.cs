using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class TwitterApi
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

        private string GetAuthorizationToken(string signature)
        {
            var values = new List<string>();
            values.Add(string.Format("oauth_consumer_key=\"{0}\"", Uri.EscapeDataString(ConsumerKey)));
            values.Add(string.Format("oauth_nonce=\"{0}\"", _nonce));
            values.Add(string.Format("oauth_signature=\"{0}\"", Uri.EscapeDataString(signature)));
            values.Add("oauth_signature_method=\"HMAC-SHA1\"");
            values.Add(string.Format("oauth_timestamp=\"{0}\"", _timestamp));
            values.Add(string.Format("oauth_token=\"{0}\"", Uri.EscapeDataString(AccessToken)));
            values.Add("oauth_version=\"1.0\"");

            return "OAuth " + string.Join(", ", values);
        }

        private string GetSignatureForRequest(string message)
        {
            var parameters = new List<string>();
            parameters.Add(string.Format("oauth_consumer_key={0}", Uri.EscapeDataString(ConsumerKey)));
            parameters.Add(string.Format("oauth_nonce={0}", _nonce));
            parameters.Add("oauth_signature_method=HMAC-SHA1");
            parameters.Add(string.Format("oauth_timestamp={0}", _timestamp));
            parameters.Add(string.Format("oauth_token={0}", Uri.EscapeDataString(AccessToken)));
            parameters.Add("oauth_version=1.0");
            parameters.Add(string.Format("status={0}", Uri.EscapeDataString(message)));

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
