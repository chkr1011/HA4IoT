using System;
using System.Text;
using FluentAssertions;
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
            parser.TryParse(buffer, out request).ShouldBeEquivalentTo(true);

            request.Method.ShouldBeEquivalentTo(HttpMethod.Delete);
            request.Uri.ShouldBeEquivalentTo("/Uri%20/lalalo323/_/-/+/%/@/&/./~/:/#/;/,/*");
            request.Body.ShouldBeEquivalentTo("Body123{}%!(:<>=");
            request.HttpVersion.ShouldBeEquivalentTo(new Version(1, 1));
            request.Headers.GetValue("Host").ShouldBeEquivalentTo("localhost:2400");
            request.Headers.GetValue("Connection").ShouldBeEquivalentTo("keep-alive");
            request.Headers.GetValue("Accept").ShouldBeEquivalentTo("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.Headers.GetValue("Upgrade-Insecure-Requests").ShouldBeEquivalentTo("1");
            request.Headers.GetValue("User-Agent").ShouldBeEquivalentTo("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36");
            request.Headers.GetValue("Accept-Encoding").ShouldBeEquivalentTo("gzip, deflate, sdch");
            request.Headers.GetValue("Accept-Language").ShouldBeEquivalentTo("de,en-US;q=0.8,en;q=0.6,de-DE;q=0.4");
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
