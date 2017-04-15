using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace HA4IoT.Net.Http
{
    public sealed class HttpRequestReader : IDisposable
    {
        private readonly MemoryStream _buffer = new MemoryStream();
        private readonly byte[] _chunkBuffer = new byte[8 * 1024]; // 8 KB
        private readonly Stream _stream;

        private HttpRequest _request;
        private long? _bodyLength;

        public HttpRequestReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public async Task<HttpRequest> TryReadAsync()
        {
            try
            {
                Reset();

                await ReadChunckFromStreamAsync();

                ParsePrefix();
                ParseHeaders();
                await ParseBody();

                return _request;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ParsePrefix()
        {
            var line = ReadLine();
            var items = line.Split(' ');

            _request.Method = ParseHttpMethod(items[0]);
            _request.Uri = items[1];

            if (items[2] != "HTTP/1.1")
            {
                throw new NotSupportedException("HTTP version not supported.");
            }

            _request.HttpVersion = new Version(1, 1);

            if (!_request.Uri.Contains("?"))
            {
                return;
            }

            var indexOfQuestionMark = _request.Uri.IndexOf('?');
            _request.Query = _request.Uri.Substring(indexOfQuestionMark + 1);
            _request.Query = Uri.UnescapeDataString(_request.Query);
            _request.Uri = _request.Uri.Substring(0, indexOfQuestionMark);
        }

        private static HttpMethod ParseHttpMethod(string source)
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

        private static KeyValuePair<string, string> ParseHeader(string source)
        {
            var delimiterIndex = source.IndexOf(':');
            if (delimiterIndex == -1)
            {
                return  new KeyValuePair<string, string>(source, null);
            }

            var name = source.Substring(0, delimiterIndex).Trim();
            var value = source.Substring(delimiterIndex + 1).Trim();

            return new KeyValuePair<string, string>(name, value);
        }

        private void ParseHeaders()
        {
            var line = ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                var header = ParseHeader(line);
                if (string.Compare(header.Key, HttpHeaderName.ContentLength, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _bodyLength = long.Parse(header.Value);
                }

                _request.Headers.Add(header.Key, header.Value);
                line = ReadLine();
            }
        }

        private async Task ParseBody()
        {
            if (!_bodyLength.HasValue)
            {
                _request.Body = ReadRemainingData();
            }
            else if (_bodyLength.Value == 0)
            {
                _request.Body = new byte[0];
            }
            else
            {
                var previousRemainingDataLength = RemainingDataLength();
                while (RemainingDataLength() < _bodyLength.Value)
                {
                    // TODO: Consider timeout here!
                    await ReadChunckFromStreamAsync();

                    if (previousRemainingDataLength == RemainingDataLength())
                    {
                        throw new InvalidOperationException();
                    }
                }

                _request.Body = ReadRemainingData();
            }
        }

        private async Task ReadChunckFromStreamAsync()
        {
            var size = await _stream.ReadAsync(_chunkBuffer, 0, _chunkBuffer.Length);
            _buffer.Write(_chunkBuffer, 0, size);
            _buffer.Position -= size;
        }

        private string ReadLine()
        {
            var buffer = new StringBuilder();

            while (!IsEndOfStream())
            {
                var @byte = _buffer.ReadByte();
                if (@byte == '\n')
                {
                    break;
                }

                if (@byte != '\r')
                {
                    buffer.Append((char)@byte);
                }
            }

            return buffer.ToString();
        }

        private long RemainingDataLength()
        {
            return _buffer.Length - _buffer.Position;
        }

        private byte[] ReadRemainingData()
        {
            var buffer = new byte[_buffer.Length - _buffer.Position];
            _buffer.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        private bool IsEndOfStream()
        {
            return _buffer.Position == _buffer.Length;
        }

        private void Reset()
        {
            _request = new HttpRequest();

            _buffer.Position = 0;
            _buffer.SetLength(0);
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }
    }
}