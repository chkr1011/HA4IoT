using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.Knx;

namespace HA4IoT.Controller.Main.Rooms
{
    internal abstract class RoomConfiguration
    {
        protected RoomConfiguration(
            IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Controller = controller;
            CCToolsBoardController = controller.GetDevice<CCToolsBoardController>();
            RemoteSocketController = controller.GetDevice<RemoteSocketController>();
        }

        public IController Controller { get; }
        public CCToolsBoardController CCToolsBoardController { get; }
        public RemoteSocketController RemoteSocketController { get; }

        public abstract void Setup();
    }
}
