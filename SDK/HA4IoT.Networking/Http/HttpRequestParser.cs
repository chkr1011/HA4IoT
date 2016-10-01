using System;
using System.Text;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public class HttpRequestParser
    {
        private const string MethodPattern = @"(?'method'GET|POST|PATCH|PUT|DELETE|TRACE)";
        private const string UriPattern = @"(?'uri'/\S+?)";
        private const string VersionPattern = @"HTTP/(?'version'1.1)";
        private const string HeadersPattern = @"(?'headers'[\w\W]*?)";
        private const string BodyPattern = @"(?'body'.*?)";

        private readonly Regex _regex;

        private readonly HttpHeaderCollection _headers = new HttpHeaderCollection();
        
        private HttpMethod _method;
        private string _uri;
        private string _httpVersion;
        private string _body;
        private string _query;

        public HttpRequestParser()
        {
            var pattern = $@"^{MethodPattern} {UriPattern} {VersionPattern}((\r\n){HeadersPattern})?((\r\n\r\n){BodyPattern})?$";
            _regex = new Regex(pattern, RegexOptions.Compiled);
        }

        public bool TryParse(byte[] buffer, int bufferLength, out HttpRequest request)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            try
            {
                var content = Encoding.UTF8.GetString(buffer, 0, bufferLength);
                var groups = _regex.Match(content).Groups;

                _method = (HttpMethod)Enum.Parse(typeof(HttpMethod), groups["method"].Value, true);
                _uri = groups["uri"].Value;
                _httpVersion = groups["version"].Value;
                ParseHeaders(groups["headers"].Value);
                _body = groups["body"].Value;
                
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

        private void ParseHeaders(string source)
        {
            _headers.Clear();

            var headers = source.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var header in headers)
            {
                if (string.IsNullOrEmpty(header))
                {
                    break;
                }

                if (!header.Contains(":"))
                {
                    _headers[header] = string.Empty;
                }
                else
                {
                    var indexOfDelimiter = header.IndexOf(":", StringComparison.Ordinal);
                    var name = header.Substring(0, indexOfDelimiter).Trim();
                    var token = header.Substring(indexOfDelimiter + 1).Trim();

                    _headers[name] = token;
                }
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