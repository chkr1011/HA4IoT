using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Hardware.GenericIOBoard
{
    public class IOBoardManager
    {
        private readonly IHttpRequestController _httpApiController;
        private readonly Dictionary<Enum, IOBoardController> _ioBoards = new Dictionary<Enum, IOBoardController>();
        private readonly INotificationHandler _notificationHandler;

        public IOBoardManager(IHttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            _httpApiController = httpApiController;
            _notificationHandler = notificationHandler;
        }

        public void Add(Enum id, IOBoardController ioBoard)
        {
            if (ioBoard == null) throw new ArgumentNullException(nameof(ioBoard));

            ExposeToApi(ioBoard);
            _ioBoards.Add(id, ioBoard);
        }

        public IBinaryInputController GetInputBoard(Enum id)
        {
            return (IBinaryInputController) _ioBoards[id];
        }

        public IBinaryOutputController GetOutputBoard(Enum id)
        {
            return (IBinaryOutputController) _ioBoards[id];
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

        private void ExposeToApi(IOBoardController ioBoard)
        {
            _httpApiController.Handle(HttpMethod.Get, "device").WithSegment(ioBoard.Id).Using(c => c.Response.Body = new JsonBody(ApiGet(ioBoard)));

            _httpApiController.Handle(HttpMethod.Put, "device")
                .WithSegment(ioBoard.Id)
                .WithRequiredJsonBody()
                .Using(c => ApiPost(ioBoard, c));

            _httpApiController.Handle(HttpMethod.Patch, "device")
                .WithSegment(ioBoard.Id)
                .WithRequiredJsonBody()
                .Using(c => ApiPatch(ioBoard, c));
        }

        private JsonObject ApiGet(IOBoardController ioBoard)
        {
            var result = new JsonObject();
            result.SetNamedValue("state", ByteArrayToJsonValue(ioBoard.GetState()));
            result.SetNamedValue("committed-state", ByteArrayToJsonValue(ioBoard.GetCommittedState()));
            return result;
        }

        private void ApiPost(IOBoardController ioBoard, HttpContext httpContext)
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

        private void ApiPatch(IOBoardController ioBoard, HttpContext httpContext)
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