using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking
{
    public class HttpRequestParser
    {
        private readonly HttpHeaderCollection _headers = new HttpHeaderCollection();
        private readonly List<string> _lines = new List<string>();
        private string _body;
        private string _httpVersion;
        private HttpMethod _method;
        private string _query;
        private string _uri;

        private string _request;

        public bool TryParse(byte[] buffer, int bufferLength, out HttpRequest request)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            
            try
            {
                _request = Encoding.UTF8.GetString(buffer, 0, bufferLength);

                request = null;

                if (!TryParsePackage())
                {
                    return false;
                }

                if (!TryParseRequestHeader())
                {
                    return false;
                }

                ParseHeaders();
                ParseBody();
                ParseQuery();

                request = new HttpRequest(_method, _uri, Version.Parse(_httpVersion), _query, _headers, _body);
                return true;
            }
            catch (Exception)
            {
                request = null;
                return false;
            }
        }

        private void ParseHeaders()
        {
            _headers.Clear();

            for (var i = 0; i < _lines.Count; i++)
            {
                var isRequestLine = i == 0;
                if (isRequestLine)
                {
                    continue;
                }

                var line = _lines[i];

                var isEmptyLine = string.IsNullOrEmpty(line);
                if (isEmptyLine)
                {
                    break;
                }

                if (!line.Contains(":"))
                {
                    _headers[line] = string.Empty;
                }
                else
                {
                    var indexOfDelimiter = line.IndexOf(":");
                    var name = line.Substring(0, indexOfDelimiter).Trim();
                    var token = line.Substring(indexOfDelimiter + 1).Trim();

                    _headers[name] = token;
                }
            }
        }

        private void ParseBody()
        {
            var bodyOffset = _request.IndexOf(Environment.NewLine + Environment.NewLine,
                StringComparison.OrdinalIgnoreCase);

            if (bodyOffset == -1)
            {
                return;
            }

            _body = _request.Substring(bodyOffset + (Environment.NewLine.Length*2));
        }

        private void ParseQuery()
        {
            if (!_uri.Contains("?"))
            {
                return;
            }

            int indexOfQuestionMark = _uri.IndexOf('?');

            _query = _uri.Substring(indexOfQuestionMark + 1);
            _uri = _uri.Substring(0, indexOfQuestionMark);
            
            // This hack is requiered to allow sending data to the server which is blocked to avoid cross site scripting.
            if (_query.StartsWith("body="))
            {
                _body = _query.Substring("body=".Length).Replace("%22", "\"").Replace("%20", " ");
                _query = null;
            }
        }

        private bool TryParsePackage()
        {
            if (string.IsNullOrEmpty(_request))
            {
                return false;
            }

            _lines.Clear();
            _lines.AddRange(_request.Split(new[] {Environment.NewLine}, StringSplitOptions.None));
            if (_lines.Count < 2)
            {
                return false;
            }

            return true;
        }

        private bool TryParseRequestHeader()
        {
            var requestParts = _lines.First().Split(new[] {' '}, StringSplitOptions.None);
            if (requestParts.Length != 3)
            {
                return false;
            }

            if (!Enum.TryParse(requestParts[0], true, out _method))
            {
                return false;
            }

            _uri = requestParts[1];
            _httpVersion = requestParts[2].Substring(requestParts[2].IndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);

            return true;
        }
    }
}