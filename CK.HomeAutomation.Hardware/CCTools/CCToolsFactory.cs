using System;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class CCToolsFactory
    {
        private readonly I2CBus _i2CBus;
        private readonly IOBoardManager _ioBoardManager;
        private readonly INotificationHandler _notificationHandler;

        public CCToolsFactory(I2CBus i2CBus, IOBoardManager ioBoardManager, INotificationHandler notificationHandler)
        {
            _i2CBus = i2CBus;
            _ioBoardManager = ioBoardManager;
            _notificationHandler = notificationHandler;
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
    }
}
