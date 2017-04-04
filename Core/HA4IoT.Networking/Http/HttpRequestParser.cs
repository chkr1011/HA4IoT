using System;
using System.IO;
using System.Text;

namespace HA4IoT.Networking.Http
{
    public class HttpRequestParser
    {
        private readonly byte[] _buffer;
        private readonly int _bufferLength;

        private readonly HttpRequest _request = new HttpRequest();

        public HttpRequestParser(byte[] buffer, int bufferLength)
        {
            _buffer = buffer;
            _bufferLength = bufferLength;
        }

        public bool TryParse(out HttpRequest request)
        {
            try
            {
                using (var memoryStream = new MemoryStream(_buffer, 0, _bufferLength))
                {
                    ParsePrefix(ReadLine(memoryStream));

                    var line = ReadLine(memoryStream);
                    while (!string.IsNullOrEmpty(line))
                    {
                        ParseHeader(line);
                        line = ReadLine(memoryStream);
                    }

                    _request.Body = new byte[memoryStream.Length - memoryStream.Position];
                    memoryStream.Read(_request.Body, 0, _request.Body.Length);
                }

                ParseQuery();

                request = _request;
                return true;
            }
            catch (Exception)
            {
                request = null;
                return false;
            }
        }

        private string ReadLine(MemoryStream memoryStream)
        {
            if (memoryStream.Position == memoryStream.Length)
            {
                return null;
            }

            using (var buffer = new MemoryStream())
            {
                while (memoryStream.Position != memoryStream.Length)
                {
                    var @byte = (byte) memoryStream.ReadByte();
                    if (@byte == '\n')
                    {
                        break;
                    }

                    if (@byte != '\r')
                    {
                        buffer.WriteByte(@byte);
                    }
                }

                return Encoding.UTF8.GetString(buffer.ToArray());
            }
        }

        private void ParsePrefix(string source)
        {
            var items = source.Split(' ');

            _request.Method = (HttpMethod)Enum.Parse(typeof(HttpMethod), items[0], true);
            _request.Uri = items[1];
            _request.HttpVersion = Version.Parse(items[2].Substring(5)); // Remove HTTP/ from HTTP/1.1
        }

        private void ParseHeader(string source)
        {
            var delimiterIndex = source.IndexOf(':');
            if (delimiterIndex == -1)
            {
                _request.Headers.Add(source, string.Empty);
            }
            else
            {
                var name = source.Substring(0, delimiterIndex).Trim();
                var value = source.Substring(delimiterIndex + 1).Trim();

                _request.Headers.Add(name, value);
            }
        }

        private void ParseQuery()
        {
            if (!_request.Uri.Contains("?"))
            {
                return;
            }

            var indexOfQuestionMark = _request.Uri.IndexOf('?');

            _request.Query = _request.Uri.Substring(indexOfQuestionMark + 1);
            _request.Uri = _request.Uri.Substring(0, indexOfQuestionMark);

            // Parse a special query parameter.
            if (!_request.Query.StartsWith("body="))
            {
                return;
            }

            _request.Body = Encoding.ASCII.GetBytes(Uri.UnescapeDataString(_request.Query.Substring("body=".Length)));
            _request.Headers[HttpHeaderNames.ContentLength] = _request.Body.Length.ToString();

            _request.Query = null;
        }
    }
}