using System;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Scripting.Proxies
{
    public class messageBrokerProxy : IScriptProxy
    {
        private readonly IMessageBrokerService _messageBrokerService;
        private readonly ScriptingSession _scriptingSession;

        [MoonSharpHidden]
        public messageBrokerProxy(IMessageBrokerService messageBrokerService, ScriptingSession scriptingSession)
        {
            _messageBrokerService = messageBrokerService ?? throw new ArgumentNullException(nameof(messageBrokerService));
            _scriptingSession = scriptingSession ?? throw new ArgumentNullException(nameof(scriptingSession));
        }

        [MoonSharpHidden]
        public string Name => "messageBroker";

        public void Subscribe(string id, string topic, string payloadType, string callbackFunctionName)
        {
            var messageSubscription = new MessageSubscription
            {
                Id = id,
                Topic = topic,
                PayloadType = payloadType,
                Callback = m => _scriptingSession.Execute(callbackFunctionName)
            };

            _messageBrokerService.Subscribe(messageSubscription);
        }

        public string[] GetSubscriptions()
        {
            //ashdajshdjsahd
            return new string[0];
        }

        public void Unsubscribe(string uid)
        {
            _messageBrokerService.Unsubscribe(uid);
        }

        public void Publish(string topic, string type, string payload)
        {
            _messageBrokerService.Publish(new Message<JObject>(topic, new MessagePayload<JObject>(type, JObject.Parse(payload))));
        }
    }
}
