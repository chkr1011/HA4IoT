using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Networking;

namespace HA4IoT.PersonalAgent
{
    public class PersonalAgentToApiDispatcher
    {
        private readonly IController _controller;

        public PersonalAgentToApiDispatcher(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        public void ExposeToApi(IApiController apiController)
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            apiController.RouteCommand("personalAgent", HandleCommand);
        }

        private void HandleCommand(IApiContext apiContext)
        {
            string message = apiContext.Request.GetNamedString("message", string.Empty);
            if (string.IsNullOrEmpty(message))
            {
                apiContext.ResultCode = ApiResultCode.InvalidBody;
                return;
            }
            
            var messageProcessor = new PersonalAgentMessageProcessor(_controller);
            messageProcessor.ProcessMessage(new ApiInboundMessage(DateTime.Now, message));

            apiContext.Response.SetNamedString("answer", messageProcessor.Answer);
        }
    }
}
