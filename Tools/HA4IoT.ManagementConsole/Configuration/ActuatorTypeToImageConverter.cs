using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HA4IoT.ManagementConsole.Configuration
{
    public class ActuatorTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageName = "ImageDefaultActuator";

            switch (System.Convert.ToString(value))
            {
                case "HA4IoT.Actuators.Lamp":
                    {
                        imageName = "ImageLamp";
                        break;
                    }

                case "HA4IoT.Actuators.Socket":
                    {
                        imageName = "ImageSocket";
                        break;
                    }

                case "HA4IoT.Actuators.TemperatureSensor":
                    {
                        imageName = "ImageTemperature";
                        break;
                    }

                case "HA4IoT.Actuators.HumiditySensor":
                    {
                        imageName = "ImageHumidity";
                        break;
                    }

                case "HA4IoT.Actuators.MotionDetector":
                    {
                        imageName = "ImageWalk";
                        break;
                    }

                case "HA4IoT.Actuators.Window":
                    {
                        imageName = "ImageWindow";
                        break;
                    }

                case "HA4IoT.Actuators.RollerShutter":
                    {
                        imageName = "ImageRollerShutter";
                        break;
                    }

                case "HA4IoT.Actuators.RollerShutterButtons":
                case "HA4IoT.Actuators.VirtualButton":
                case "HA4IoT.Actuators.Button":
                    {
                        imageName = "ImageButton";
                        break;
                    }
            }

            return Application.Current.FindResource(imageName) as Geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
