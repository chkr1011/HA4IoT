using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.RemoteSwitch;

namespace HA4IoT.Controller.Main.Rooms
{
    internal abstract class RoomConfiguration
    {
        public RoomConfiguration(
            IController controller, 
            CCToolsBoardController ccToolsBoardController,
            RemoteSocketController remoteSocketController)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (ccToolsBoardController == null) throw new ArgumentNullException(nameof(ccToolsBoardController));

            Controller = controller;
            CCToolsBoardController = ccToolsBoardController;
            RemoteSocketController = remoteSocketController;
        }

        public IController Controller { get; }
        public CCToolsBoardController CCToolsBoardController { get; }
        public RemoteSocketController RemoteSocketController { get; set; }

        public abstract void Setup();
    }
}
