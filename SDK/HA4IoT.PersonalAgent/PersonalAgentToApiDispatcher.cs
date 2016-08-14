using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;

namespace HA4IoT.PersonalAgent
{
    public class PersonalAgentToApiDispatcher
    {
        private readonly IContainerService _factoryService;
        
        public PersonalAgentToApiDispatcher(IContainerService factoryService)
        {
            if (_factoryService == null) throw new ArgumentNullException(nameof(factoryService));

            _factoryService = factoryService;
        }

        public void ExposeToApi(IApiService apiController)
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
            
            var messageProcessor = _factoryService.GetInstance<PersonalAgentMessageProcessor>();
            messageProcessor.ProcessMessage(new ApiInboundMessage(DateTime.Now, message));

            apiContext.Response.SetNamedString("answer", messageProcessor.Answer);
        }
    }
}
