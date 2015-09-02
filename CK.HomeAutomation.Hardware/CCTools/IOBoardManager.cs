using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Data.Json;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class IOBoardManager
    {
        private readonly Dictionary<Enum, IOBoardController> _ioBoards = new Dictionary<Enum, IOBoardController>();
        private readonly HttpRequestController _httpApiController;
        private readonly INotificationHandler _notificationHandler;

        public IOBoardManager(HttpRequestController httpApiController, INotificationHandler notificationHandler)
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

        public IInputController GetInputBoard(Enum id)
        {
            return (IInputController) _ioBoards[id];
        }

        public IOutputController GetOutputBoard(Enum id)
        {
            return (IOutputController) _ioBoards[id];
        }

        public void PollInputBoardStates()
        {
            var stopwatch = Stopwatch.StartNew();
            foreach (var portExpanderController in _ioBoards.Values)
            {
                if (portExpanderController.AutomaticallyFetchState)
                {
                    portExpanderController.FetchState();
                }
            }

            stopwatch.Stop();
            _notificationHandler.Publish(NotificationType.Verbose, "Fetching inputs took {0}ms.", stopwatch.ElapsedMilliseconds);
        }

        private void ExposeToApi(IOBoardController ioBoard)
        {
            _httpApiController.Handle(HttpMethod.Get, "device").WithSegment(ioBoard.Id).Using(c => c.Response.Result = GetJSONState(ioBoard));

            _httpApiController.Handle(HttpMethod.Put, "device")
                .WithSegment(ioBoard.Id)
                .WithRequiredJsonBody()
                .Using(c => ApplyJSONState(ioBoard, c.Request.JsonBody));

            _httpApiController.Handle(HttpMethod.Patch, "device")
                .WithSegment(ioBoard.Id)
                .WithRequiredJsonBody()
                .Using(c => ApplyJSONPortState(ioBoard, c.Request.JsonBody));
        }

        private JsonObject GetJSONState(IOBoardController ioBoard)
        {
            var result = new JsonObject();
            result.SetNamedValue("state", ByteArrayToJSONValue(ioBoard.GetState()));
            result.SetNamedValue("committed-state", ByteArrayToJSONValue(ioBoard.GetCommittedState()));
            return result;
        }

        private void ApplyJSONState(IOBoardController ioBoard, JsonObject value)
        {
            JsonArray state = value.GetNamedArray("state", null);
            if (state != null)
            {
                byte[] buffer = JSONValueToByteArray(state);
                ioBoard.SetState(buffer);
            }

            var commit = value.GetNamedBoolean("commit", true);
            if (commit)
            {
                ioBoard.CommitChanges();
            }
        }

        private void ApplyJSONPortState(IOBoardController ioBoard, JsonObject value)
        {
            int port = (int) value.GetNamedNumber("port", 0);
            bool state = value.GetNamedBoolean("state", false);
            bool commit = value.GetNamedBoolean("commit", true);

            ioBoard.SetPortState(port, state ? BinaryState.High : BinaryState.Low);

            if (commit)
            {
                ioBoard.CommitChanges();
            }
        }

        private JsonArray ByteArrayToJSONValue(byte[] data)
        {
            var value = new JsonArray();
            foreach (var item in data)
            {
                value.Add(JsonValue.CreateNumberValue(item));
            }

            return value;
        }

        private byte[] JSONValueToByteArray(JsonArray value)
        {
            return value.Select(item => (byte) item.GetNumber()).ToArray();
        }
    }
}