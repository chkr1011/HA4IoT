using System;
using System.Text;
using FluentAssertions;
using HA4IoT.Contracts.Networking;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Networking.Tests
{
    [TestClass]
    public class HttpRequestParserTests
    {
        [TestMethod]
        public void Parse_HttpRequest()
        {
            var parser = new HttpRequestParser();

            byte[] buffer = Encoding.UTF8.GetBytes(GetRequestText());

            HttpRequest request;
            parser.TryParse(buffer, buffer.Length, out request).ShouldBeEquivalentTo(true);

            request.Method.ShouldBeEquivalentTo(HttpMethod.Delete);
            request.Uri.ShouldBeEquivalentTo("/Uri%20/lalalo323/_/-/+/%/@/&/./~/:/#/;/,/*");
            request.Body.ShouldBeEquivalentTo("Body123{}%!(:<>=");
            request.HttpVersion.ShouldBeEquivalentTo(new Version(1, 1));
            request.Headers["Host"].ShouldBeEquivalentTo("localhost:2400");
            request.Headers["Connection"].ShouldBeEquivalentTo("keep-alive");
            request.Headers["Accept"].ShouldBeEquivalentTo("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.Headers["Upgrade-Insecure-Requests"].ShouldBeEquivalentTo("1");
            request.Headers["User-Agent"].ShouldBeEquivalentTo("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36");
            request.Headers["Accept-Encoding"].ShouldBeEquivalentTo("gzip, deflate, sdch");
            request.Headers["Accept-Language"].ShouldBeEquivalentTo("de,en-US;q=0.8,en;q=0.6,de-DE;q=0.4");
        }

        private string GetRequestText()
        {
            return @"DELETE /Uri%20/lalalo323/_/-/+/%/@/&/./~/:/#/;/,/* HTTP/1.1
Host: localhost:2400
Connection: keep-alive
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36
Accept-Encoding: gzip, deflate, sdch
Accept-Language: de,en-US;q=0.8,en;q=0.6,de-DE;q=0.4

Body123{}%!(:<>=";
        }
    }
}
