using System;
using System.IO;
using System.Text;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public class HttpRequestParser
    {
        private readonly HttpHeaderCollection _headers = new HttpHeaderCollection();
        
        private HttpMethod _method;
        private string _uri;
        private string _httpVersion;
        private string _body;
        private string _query;

        public bool TryParse(byte[] buffer, int bufferLength, out HttpRequest request)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            
            try
            {
                using (var memoryStream = new MemoryStream(buffer, 0, bufferLength))
                using (var streamReader = new StreamReader(memoryStream))
                {
                    ParseFirstLine(streamReader.ReadLine());

                    var line = streamReader.ReadLine();
                    while (!streamReader.EndOfStream && !string.IsNullOrEmpty(line))
                    {
                        ParseHeader(line);
                        line = streamReader.ReadLine();
                    }

                    _body = streamReader.ReadToEnd();
                }

                ParseQuery();

                var binaryBodyLength = 0;
                if (!string.IsNullOrEmpty(_body))
                {
                    binaryBodyLength = Encoding.UTF8.GetByteCount(_body);
                }

                request = new HttpRequest(_method, _uri, Version.Parse(_httpVersion), _query, _headers, _body, binaryBodyLength);
                return true;
            }
            catch (Exception)
            {
                request = null;
                return false;
            }
        }

        private void ParseFirstLine(string source)
        {
            var items = source.Split(' ');

            _method = (HttpMethod)Enum.Parse(typeof(HttpMethod), items[0], true);
            _uri = items[1];
            _httpVersion = items[2].Substring(5); // Remove HTTP/ from HTTP/1.1
        }

        private void ParseHeader(string source)
        {
            var items = source.Split(':');
            if (items.Length == 1)
            {
                _headers[source] = string.Empty;
            }
            else
            {
                _headers[items[0].Trim()] = items[1].Trim();
            }
        }

        private void ParseQuery()
        {
            if (!_uri.Contains("?"))
            {
                return;
            }

            var indexOfQuestionMark = _uri.IndexOf('?');

            _query = _uri.Substring(indexOfQuestionMark + 1);
            _uri = _uri.Substring(0, indexOfQuestionMark);

            // Parse a special query parameter.
            if (!_query.StartsWith("body="))
            {
                return;
            }

            _body = Uri.UnescapeDataString(_query.Substring("body=".Length));
            _headers[HttpHeaderNames.ContentLength] = _body.Length.ToString();

            _query = null;
        }
    }
}