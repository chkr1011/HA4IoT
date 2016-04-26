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
                case "Lamp":
                    {
                        imageName = "ImageLamp";
                        break;
                    }

                case "Socket":
                    {
                        imageName = "ImageSocket";
                        break;
                    }

                case "TemperatureSensor":
                    {
                        imageName = "ImageTemperature";
                        break;
                    }

                case "HumiditySensor":
                    {
                        imageName = "ImageHumidity";
                        break;
                    }

                case "MotionDetector":
                    {
                        imageName = "ImageWalk";
                        break;
                    }

                case "Window":
                    {
                        imageName = "ImageWindow";
                        break;
                    }

                case "RollerShutter":
                    {
                        imageName = "ImageRollerShutter";
                        break;
                    }

                case "RollerShutterButtons":
                case "VirtualButton":
                case "Button":
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
