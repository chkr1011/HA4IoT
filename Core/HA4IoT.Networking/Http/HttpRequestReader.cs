using System;
using System.IO;
using System.Text;
using Windows.Web.Http;

namespace HA4IoT.Networking.Http
{
    public class HttpRequestReader
    {
        private readonly byte[] _buffer;
        private readonly int _bufferLength;

        private readonly HttpRequest _request = new HttpRequest();

        public HttpRequestReader(byte[] buffer, int bufferLength)
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

        private string ReadLine(Stream memoryStream)
        {
            if (memoryStream.Position == memoryStream.Length)
            {
                return null;
            }

            using (var buffer = new MemoryStream())
            {
                while (memoryStream.Position != memoryStream.Length)
                {
                    var @byte = (byte)memoryStream.ReadByte();
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
            _request.Method = ParseHttpMethod(items[0]);
            _request.Uri = items[1];

            if (items[2] != "HTTP/1.1")
            {
                throw new NotSupportedException("HTTP version not supported.");
            }

            _request.HttpVersion = new Version(1, 1);
        }

        private HttpMethod ParseHttpMethod(string source)
        {
            switch (source.ToUpperInvariant())
            {
                case "GET": return HttpMethod.Get;
                case "POST": return HttpMethod.Post;
                case "DELETE": return HttpMethod.Delete;
                case "PUT": return HttpMethod.Put;
                case "PATCH": return HttpMethod.Patch;
                case "OPTIONS": return HttpMethod.Options;
                case "HEAD": return HttpMethod.Head;

                default:
                    {
                        throw new NotSupportedException("HTTP method not supported.");
                    }
            }
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
            _request.Query = Uri.UnescapeDataString(_request.Query);

            _request.Uri = _request.Uri.Substring(0, indexOfQuestionMark);
        }
    }
}