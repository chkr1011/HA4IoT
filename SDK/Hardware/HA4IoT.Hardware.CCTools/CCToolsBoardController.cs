using System;
using System.Diagnostics;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsBoardController
    {
        private readonly II2CBus _i2CBus;
        private readonly IController _controller;
        private readonly IHttpRequestController _httpApi;
        private readonly INotificationHandler _logger;

        public CCToolsBoardController(IController controller, II2CBus i2cBus, IHttpRequestController httpApi, INotificationHandler logger)
        {
            if (i2cBus == null) throw new ArgumentNullException(nameof(i2cBus));
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (httpApi == null) throw new ArgumentNullException(nameof(httpApi));

            _controller = controller;
            _i2CBus = i2cBus;
            
            _httpApi = httpApi;
            _logger = logger;
        }

        public HSPE16InputOnly CreateHSPE16InputOnly(Enum id, I2CSlaveAddress address)
        {
            var device = new HSPE16InputOnly(DeviceId.From(id), address, _i2CBus, _httpApi, _logger)
            {
                AutomaticallyFetchState = true
            };

            _controller.AddDevice(device);

            return device;
        }

        public HSPE16OutputOnly CreateHSPE16OutputOnly(Enum id, I2CSlaveAddress address)
        {
            var device = new HSPE16OutputOnly(DeviceId.From(id), address, _i2CBus, _httpApi, _logger);
            _controller.AddDevice(device);

            return device;
        }

        public HSPE8OutputOnly CreateHSPE8OutputOnly(Enum id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSPE8OutputOnly(DeviceId.From(id), i2CAddress, _i2CBus, _httpApi, _logger);
            _controller.AddDevice(device);

            return device;
        }

        public HSPE8InputOnly CreateHSPE8InputOnly(Enum id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSPE8InputOnly(DeviceId.From(id), i2CAddress, _i2CBus, _httpApi, _logger);
            _controller.AddDevice(device);

            return device;
        }

        public HSREL5 CreateHSREL5(Enum id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSREL5(DeviceId.From(id), i2CAddress, _i2CBus, _httpApi, _logger);
            _controller.AddDevice(device);

            return device;
        }

        public HSREL8 CreateHSREL8(Enum id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSREL8(DeviceId.From(id), i2CAddress, _i2CBus, _httpApi, _logger);
            _controller.AddDevice(device);

            return device;
        }

        public HSRT16 CreateHSRT16(Enum id, I2CSlaveAddress address)
        {
            var device = new HSRT16(DeviceId.From(id), address, _i2CBus, _httpApi, _logger);
            _controller.AddDevice(device);

            return device;
        }

        public void PollInputBoardStates()
        {
            var stopwatch = Stopwatch.StartNew();

            var inputDevices = _controller.GetDevices<CCToolsInputBoardBase>();

            foreach (var portExpanderController in inputDevices)
            {
                if (portExpanderController.AutomaticallyFetchState)
                {
                    portExpanderController.PeekState();
                }
            }

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 25)
            {
                _logger.Publish(NotificationType.Warning, "Fetching inputs took {0}ms.", stopwatch.ElapsedMilliseconds);
            }

            foreach (var portExpanderController in inputDevices)
            {
                if (portExpanderController.AutomaticallyFetchState)
                {
                    portExpanderController.FetchState();
                }
            }
        }
    }
}
