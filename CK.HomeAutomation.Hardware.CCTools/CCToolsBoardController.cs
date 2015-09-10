using System;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class CCToolsBoardController
    {
        private readonly I2CBus _i2CBus;
        private readonly IOBoardManager _ioBoardManager;
        private readonly INotificationHandler _notificationHandler;

        public CCToolsBoardController(I2CBus i2CBus, HttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            _i2CBus = i2CBus;
            _notificationHandler = notificationHandler;

            _ioBoardManager = new IOBoardManager(httpApiController, notificationHandler);
        }

        public HSPE16InputOnly CreateHSPE16InputOnly(Enum id, int address)
        {
            var device = new HSPE16InputOnly(id.ToString(), address, _i2CBus, _notificationHandler) { AutomaticallyFetchState = true };
            _ioBoardManager.Add(id, device);

            return device;
        }

        public HSPE16OutputOnly CreateHSPE16OutputOnly(Enum id, int address)
        {
            var device = new HSPE16OutputOnly(id.ToString(), address, _i2CBus, _notificationHandler) { AutomaticallyFetchState = true };
            _ioBoardManager.Add(id, device);

            return device;
        }

        public HSPE8 CreateHSPE8OutputOnly(Enum id, int address)
        {
            var device = new HSPE8(id.ToString(), address, _i2CBus, _notificationHandler);
            _ioBoardManager.Add(id, device);

            return device;
        }

        public HSREL5 CreateHSREL5(Enum id, int address)
        {
            var device = new HSREL5(id.ToString(), address, _i2CBus, _notificationHandler);
            _ioBoardManager.Add(id, device);

            return device;
        }

        public HSREL8 CreateHSREL8(Enum id, int address)
        {
            var device = new HSREL8(id.ToString(), address, _i2CBus, _notificationHandler);
            _ioBoardManager.Add(id, device);

            return device;
        }

        public HSRT16 CreateHSRT16(Enum id, int address)
        {
            var device = new HSRT16(id.ToString(), address, _i2CBus, _notificationHandler);
            _ioBoardManager.Add(id, device);

            return device;
        }

        public void PollInputBoardStates()
        {
            _ioBoardManager.PollInputBoardStates();
        }

        public IInputController GetInputBoard(Enum id)
        {
            return _ioBoardManager.GetInputBoard(id);
        }

        public IOutputController GetOutputBoard(Enum id)
        {
            return _ioBoardManager.GetOutputBoard(id);
        }
    }
}
