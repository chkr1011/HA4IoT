using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.GenericIOBoard
{
    public class IOBoardCollection
    {
        private readonly Dictionary<Enum, IOBoardControllerBase> _ioBoards = new Dictionary<Enum, IOBoardControllerBase>();

        private readonly IHttpRequestController _httpApiController;
        private readonly INotificationHandler _notificationHandler;

        public IOBoardCollection(IHttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            _httpApiController = httpApiController;
            _notificationHandler = notificationHandler;
        }

        public void Add(Enum id, IOBoardControllerBase ioBoard)
        {
            if (ioBoard == null) throw new ArgumentNullException(nameof(ioBoard));

            if (_ioBoards.ContainsKey(id))
            {
                throw new InvalidOperationException("Input board with ID '" + id + "' already registered.");
            }

            ExposeIoBoardToApi(ioBoard);
            _ioBoards.Add(id, ioBoard);
        }

        public IBinaryInputController GetInputBoard(Enum id)
        {
            IOBoardControllerBase board;
            if (TryGetIOBoard(id, out board))
            {
                return (IBinaryInputController)board;
            }

            throw new InvalidOperationException("No input board with ID '" + id + "' registered.");
        }

        public IBinaryOutputController GetOutputBoard(Enum id)
        {
            IOBoardControllerBase board;
            if (TryGetIOBoard(id, out board))
            {
                return (IBinaryOutputController)board;
            }

            throw new InvalidOperationException("No output board with ID '" + id + "' registered.");
        }

        public bool TryGetIOBoard(Enum id, out IOBoardControllerBase board)
        {
            IOBoardControllerBase buffer;
            if (_ioBoards.TryGetValue(id, out buffer))
            {
                board = buffer;
                return true;
            }

            board = null;
            return false;
        }

        public void PollInputBoardStates()
        {
            var stopwatch = Stopwatch.StartNew();

            foreach (var portExpanderController in _ioBoards.Values)
            {
                if (portExpanderController.AutomaticallyFetchState)
                {
                    portExpanderController.PeekState();
                }
            }

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 25)
            {
                _notificationHandler.Publish(NotificationType.Warning, "Fetching inputs took {0}ms.", stopwatch.ElapsedMilliseconds);
            }
            
            foreach (var portExpanderController in _ioBoards.Values)
            {
                if (portExpanderController.AutomaticallyFetchState)
                {
                    portExpanderController.FetchState();
                }
            }
        }

        private void ExposeIoBoardToApi(IOBoardControllerBase ioBoard)
        {
            _httpApiController.Handle(HttpMethod.Get, "device").WithSegment(ioBoard.Id).Using(c => HandleApiGet(c, ioBoard));
            _httpApiController.Handle(HttpMethod.Post, "device").WithSegment(ioBoard.Id).Using(c => HandleApiPost(c, ioBoard));
            _httpApiController.Handle(HttpMethod.Patch, "device").WithSegment(ioBoard.Id).Using(c => HandleApiPatch(c, ioBoard));
        }

        private void HandleApiGet(HttpContext httpContext, IOBoardControllerBase ioBoard)
        {
            var result = new JsonObject();
            result.SetNamedValue("state", ByteArrayToJsonValue(ioBoard.GetState()));
            result.SetNamedValue("committed-state", ByteArrayToJsonValue(ioBoard.GetCommittedState()));
            
            httpContext.Response.Body = new JsonBody(result);
        }

        private void HandleApiPost(HttpContext httpContext, IOBoardControllerBase ioBoard)
        {
            JsonObject requestData;
            if (!JsonObject.TryParse(httpContext.Request.Body, out requestData))
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            JsonArray state = requestData.GetNamedArray("state", null);
            if (state != null)
            {
                byte[] buffer = JsonValueToByteArray(state);
                ioBoard.SetState(buffer);
            }

            var commit = requestData.GetNamedBoolean("commit", true);
            if (commit)
            {
                ioBoard.CommitChanges();
            }
        }

        private void HandleApiPatch(HttpContext httpContext, IOBoardControllerBase ioBoard)
        {
            JsonObject requestData;
            if (!JsonObject.TryParse(httpContext.Request.Body, out requestData))
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            int port = (int)requestData.GetNamedNumber("port", 0);
            bool state = requestData.GetNamedBoolean("state", false);
            bool commit = requestData.GetNamedBoolean("commit", true);

            ioBoard.SetPortState(port, state ? BinaryState.High : BinaryState.Low);

            if (commit)
            {
                ioBoard.CommitChanges();
            }
        }

        private JsonArray ByteArrayToJsonValue(byte[] data)
        {
            var value = new JsonArray();
            foreach (var item in data)
            {
                value.Add(JsonValue.CreateNumberValue(item));
            }

            return value;
        }

        private byte[] JsonValueToByteArray(JsonArray value)
        {
            return value.Select(item => (byte) item.GetNumber()).ToArray();
        }
    }
}