using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Hardware.GenericIOBoard;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsBoardController
    {
        private readonly II2CBus _i2CBus;
        private readonly IOBoardCollection _ioBoardCollection;
        private readonly INotificationHandler _log;

        public CCToolsBoardController(II2CBus i2CBus, IOBoardCollection ioBoardCollection, INotificationHandler log)
        {
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));
            if (ioBoardCollection == null) throw new ArgumentNullException(nameof(ioBoardCollection));

            _i2CBus = i2CBus;
            _log = log;

            _ioBoardCollection = ioBoardCollection;
        }

        public HSPE16InputOnly CreateHSPE16InputOnly(Enum id, I2CSlaveAddress address)
        {
            IOBoardControllerBase board;
            if (_ioBoardCollection.TryGetIOBoard(id, out board))
            {
                return (HSPE16InputOnly) board;
            }

            var device = new HSPE16InputOnly(id.ToString(), address, _i2CBus, _log) { AutomaticallyFetchState = true };
            _ioBoardCollection.Add(id, device);

            return device;
        }

        public HSPE16OutputOnly CreateHSPE16OutputOnly(Enum id, I2CSlaveAddress address)
        {
            IOBoardControllerBase board;
            if (_ioBoardCollection.TryGetIOBoard(id, out board))
            {
                return (HSPE16OutputOnly) board;
            }

            var device = new HSPE16OutputOnly(id.ToString(), address, _i2CBus, _log);
            _ioBoardCollection.Add(id, device);

            return device;
        }

        public HSPE8 CreateHSPE8OutputOnly(Enum id, I2CSlaveAddress i2CAddress)
        {
            IOBoardControllerBase board;
            if (_ioBoardCollection.TryGetIOBoard(id, out board))
            {
                return (HSPE8)board;
            }

            var device = new HSPE8(id.ToString(), i2CAddress, _i2CBus, _log);
            _ioBoardCollection.Add(id, device);

            return device;
        }

        public HSREL5 CreateHSREL5(Enum id, I2CSlaveAddress i2CAddress)
        {
            IOBoardControllerBase board;
            if (_ioBoardCollection.TryGetIOBoard(id, out board))
            {
                return (HSREL5)board;
            }

            var device = new HSREL5(id.ToString(), i2CAddress, _i2CBus, _log);
            _ioBoardCollection.Add(id, device);

            return device;
        }

        public HSREL8 CreateHSREL8(Enum id, I2CSlaveAddress i2CAddress)
        {
            IOBoardControllerBase board;
            if (_ioBoardCollection.TryGetIOBoard(id, out board))
            {
                return (HSREL8)board;
            }

            var device = new HSREL8(id.ToString(), i2CAddress, _i2CBus, _log);
            _ioBoardCollection.Add(id, device);

            return device;
        }

        public HSRT16 CreateHSRT16(Enum id, I2CSlaveAddress address)
        {
            IOBoardControllerBase board;
            if (_ioBoardCollection.TryGetIOBoard(id, out board))
            {
                return (HSRT16)board;
            }

            var device = new HSRT16(id.ToString(), address, _i2CBus, _log);
            _ioBoardCollection.Add(id, device);

            return device;
        }
    }
}
