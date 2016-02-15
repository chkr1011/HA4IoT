using System.Diagnostics;
using Windows.Data.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Networking.Tests
{
    [TestClass]
    public class JsonObjectExtensionsTests
    {
        [TestMethod]
        public void String_ToJsonValue()
        {
            IJsonValue jsonValue = "Test".ToJsonValue();
            (jsonValue == null).ShouldBeEquivalentTo(false);
            (jsonValue.ValueType == JsonValueType.String).ShouldBeEquivalentTo(true);
            jsonValue.GetString().ShouldBeEquivalentTo("Test");
        }
        
        [TestMethod]
        public void Number_ToJsonValue()
        {
            IJsonValue jsonValue = 1.6D.ToJsonValue();
            (jsonValue == null).ShouldBeEquivalentTo(false);
            (jsonValue.ValueType == JsonValueType.Number).ShouldBeEquivalentTo(true);
            jsonValue.GetNumber().ShouldBeEquivalentTo(1.6D);
        }

        [TestMethod]
        public void Bool_ToJsonValue()
        {
            IJsonValue jsonValue = true.ToJsonValue();
            (jsonValue == null).ShouldBeEquivalentTo(false);
            (jsonValue.ValueType == JsonValueType.Boolean).ShouldBeEquivalentTo(true);
            jsonValue.GetBoolean().ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void Null_ToJsonValue()
        {
            IJsonValue jsonValue = ((object)null).ToJsonValue();
            (jsonValue == null).ShouldBeEquivalentTo(false);
            (jsonValue.ValueType == JsonValueType.Null).ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void ToJsonObject()
        {
            var data = new Data();
            data.id = "123abc";
            data.flag = true;
            data.number = 11.11M;

            var stopwatch = Stopwatch.StartNew();
            var jsonObject = data.ToJsonObject();
            stopwatch.Stop();

            jsonObject.Stringify().ShouldBeEquivalentTo("{\"id\":\"123abc\",\"number\":11.11,\"flag\":true}");

            Debug.WriteLine("Serializing object to JSON object took " + stopwatch.Elapsed.TotalMilliseconds + "ms.");
        }

        private class Data
        {
            public string id { get; set; }
            public decimal number { get; set; }
            public bool flag { get; set; }
        }
    }
}
